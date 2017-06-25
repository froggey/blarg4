using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class BasicUnit: Component {
    IMotor motor;
    IWeapon[] weapons;
    DReal weaponRange;

    Entity attackTarget = null;

    public BasicUnit(Entity entity, ComponentPrototype proto): base(entity) {
    }

    public override void OnCreate() {
        motor = entity.GetComponent<IMotor>();
        weapons = entity.GetComponents<IWeapon>().ToArray();
        weaponRange = weapons.Aggregate((DReal)0, (x, y) => DReal.Max(x,y.Range()));
        Logger.Log("Weapon range is {0}", weaponRange);
    }

    public override void OnTick() {
        if(attackTarget != null) {
            if(!attackTarget.isAlive) {
                StopCommand();
                return;
            }
            var dist = entity.Range(attackTarget);

            if(dist < weaponRange) {
                // In range, fire!
                motor.Stop();
                foreach(var weapon in weapons) {
                    weapon.FireAt(attackTarget);
                }
            } else {
                // Not in range, move to target.
                motor.MoveTo(attackTarget.position);
            }
        }
    }

    public override void StopCommand() {
        motor.Stop();
        attackTarget = null;
    }

    public override void MoveCommand(DVector3 position) {
        motor.MoveTo(position);
        attackTarget = null;
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
