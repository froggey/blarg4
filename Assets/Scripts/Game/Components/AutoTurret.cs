using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class AutoTurret: Component {
    IWeapon[] weapons;
    DReal weaponRange;

    Entity attackTarget = null;

    public AutoTurret(Entity entity, ComponentPrototype proto): base(entity) {
    }

    public override void OnCreate() {
        weapons = entity.GetComponents<IWeapon>().ToArray();
        weaponRange = weapons.Aggregate((DReal)0, (x, y) => DReal.Max(x,y.Range()));
        Logger.Log("Weapon range is {0}", weaponRange);
    }

    public override void OnTick() {
        if(attackTarget != null && !attackTarget.isAlive) {
            attackTarget = null;
        }
        if(attackTarget != null) {
            var range = entity.Range(attackTarget);
            if(range > weaponRange) {
                attackTarget = null;
            }
        }
        if(attackTarget == null) {
            attackTarget = World.current
                .FindEntitiesWithinRadius(entity.position, weaponRange, entity.team)
                .FirstOrDefault();
        }
        if(attackTarget != null) {
            foreach(var weapon in weapons) {
                weapon.FireAt(attackTarget);
            }
        }
    }

    public override void StopCommand() {
        attackTarget = null;
    }

    public override void MoveCommand(DVector3 position) {
        attackTarget = null;
    }

    public override void AttackCommand(Entity target) {
        attackTarget = target;
    }

    public override uint Checksum() {
        return 0;
    }
}

}
