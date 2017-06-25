using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class HitscanWeapon: Component, IWeapon {
    public DReal range;

    // damage to deliver.
    // Damage per second in sustain mode.
    // Total damage in burst mode.
    public DReal damage;
    // Time between shots.
    public DReal fireRate;
    // If sustainTime is 0, the weapon will act in burst mode and
    // deliver damage in one block.
    // Otherwise damage represents damage per second and will
    // be delivered over the sustain time.
    public DReal sustainTime;
    // Graphical effect to display.
    public string effect;

    public DReal refireTime;
    public DReal sustainRemaining;
    public Entity sustainTarget;

    public HitscanWeapon(Entity entity, ComponentPrototype proto): base(entity) {
        range = DReal.Parse(proto.data["range"]);
        damage = DReal.Parse(proto.data["damage"]);
        fireRate = DReal.Parse(proto.data["fireRate"]);
        sustainTime = DReal.Parse(proto.data["sustainTime"]);
        effect = proto.data["effect"];

        refireTime = 0;
        sustainRemaining = 0;
        sustainTarget = null;
    }

    public DReal Range() {
        return range;
    }

    public override void OnTick() {
        if(refireTime > 0) {
            refireTime -= World.deltaTime;
        }
        if(sustainTarget != null) {
            if(!sustainTarget.isAlive) {
                sustainTarget = null;
                refireTime = fireRate + sustainRemaining;
                return;
            }
            var dist = entity.Range(sustainTarget);
            Logger.Log("distance {0} range {1} target {3} possible damage {2}",
                       dist, range, damage * World.deltaTime,
                       entity.modelName);
            if(dist <= range) {
                sustainTarget.Damage(damage * World.deltaTime);
            }
            sustainRemaining -= World.deltaTime;
            if(sustainRemaining <= 0) {
                sustainTarget = null;
                refireTime = fireRate;
            }
        }
    }

    public void FireAt(Entity target) {
        if(refireTime > 0 || sustainTarget != null) {
            return;
        }

        var dist = entity.Range(target);
        if(dist > range) {
            return;
        }

        World.current.eventListener.Animate(entity, "Fire");
        if(sustainTime == 0) {
            target.Damage(damage);
            refireTime = fireRate;
            World.current.eventListener.HitscanBurstFire(this, effect, target);
        } else {
            World.current.eventListener.HitscanSustainedFire(this, effect, target, sustainTime);
            sustainRemaining = sustainTime;
            sustainTarget = target;
        }
    }

    public override uint Checksum() {
        return 0;
    }
}

}
