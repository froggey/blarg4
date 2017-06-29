using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class BasicUnit: Component {
    IMotor motor;
    IWeapon[] weapons;

    public DReal weaponRange { get; private set; }

    Entity attackTarget = null;
    Entity passiveAttackTarget = null;

    public enum Stance {
        Active, Passive
    }

    public Stance stance;

    public BasicUnit(Entity entity, ComponentPrototype proto): base(entity) {
    }

    public override void OnCreate() {
        motor = entity.GetComponent<IMotor>();
        weapons = entity.GetComponents<IWeapon>().ToArray();
        weaponRange = weapons.Aggregate((DReal)0, (x, y) => DReal.Max(x,y.Range()));
        Logger.Log("Weapon range is {0}", weaponRange);

        if(weapons.Length == 0) {
            stance = Stance.Passive;
        } else {
            stance = Stance.Active;
        }
    }

    public override void OnTick() {
        if(attackTarget != null) {
            if(!attackTarget.isAlive) {
                attackTarget = null;
                StopCommand();
                return;
            }
            var dist = entity.Range(attackTarget);

            if(dist < weaponRange) {
                // In range, fire!
                if(motor != null) {
                    motor.Stop();
                }
                foreach(var weapon in weapons) {
                    weapon.FireAt(attackTarget);
                }
            } else {
                // Not in range, move to target.
                if(motor != null) {
                    motor.MoveTo(attackTarget.position);
                } else {
                    // Turret behaviour, if there's no motor then it can't follow.
                    // Just forget about the target.
                    attackTarget = null;
                }
            }
        } else {
            if(stance == Stance.Active) {
                var trueRange = weaponRange;
                var collider = entity.GetComponent<Collider>();
                if(collider != null) {
                    trueRange += collider.radius;
                }

                if(passiveAttackTarget != null && !passiveAttackTarget.isAlive) {
                    passiveAttackTarget = null;
                }
                if(passiveAttackTarget != null) {
                    var range = entity.Range(passiveAttackTarget);
                    if(range > trueRange) {
                        passiveAttackTarget = null;
                    }
                }
                if(passiveAttackTarget == null) {
                    passiveAttackTarget = World.current
                        .FindEntitiesWithinRadius(entity.position, trueRange, entity.team)
                        .FirstOrDefault();
                }
                if(passiveAttackTarget != null) {
                    foreach(var weapon in weapons) {
                        weapon.FireAt(passiveAttackTarget);
                    }
                }
            }
        }
    }

    public override void StopCommand() {
        if(motor != null) {
            motor.Stop();
        }
        attackTarget = null;
        passiveAttackTarget = null;
    }

    public override void MoveCommand(DVector3 position) {
        if(motor != null) {
            motor.MoveTo(position);
        }
        attackTarget = null;
        passiveAttackTarget = null;
    }

    public override void AttackCommand(Entity target) {
        if(weapons.Length == 0) {
            return;
        }
        StopCommand();
        attackTarget = target;
    }

    public override uint Checksum() {
        return 0;
    }
}

}
