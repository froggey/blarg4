using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Projectile: Component {
    public Entity spawner;

    public DReal speed;

    public DReal directDamage;
    public DReal splashDamage;
    public DReal splashRadius;

    public DVector3 target;

    public Projectile(Entity entity, ComponentPrototype proto): base(entity) {
        speed = DReal.Parse(proto.data["speed"]);
        directDamage = DReal.Parse(proto.data["directDamage"]);
        splashDamage = DReal.Parse(proto.data["splashDamage"]);
        splashRadius = DReal.Parse(proto.data["splashRadius"]);
    }

    public override void OnTick() {
        var dir = World.current.map.Direction(entity.position, target);
        var dist = World.current.map.Distance(entity.position, target);
        var move_dist = speed * World.deltaTime;
        var next_pos = entity.position + dir * move_dist;
        DVector3 hit_pos;
        var hit_ent = World.current.LineCast(entity.position, next_pos, out hit_pos, entity.team);
        if(hit_ent != null && hit_ent != spawner) {
            Logger.Log("Hit entity {0} at {1}", hit_ent, hit_pos);
            hit_ent.Damage(directDamage, entity);
            Splash(entity.position);
            return;
        }
        entity.position = next_pos;
        entity.faceDirection = dir;
        if(dist < move_dist) {
            entity.position = target;
            Splash(entity.position);
            return;
        }
        if(entity.position.y < World.current.map.Height(entity.position)) {
            Splash(entity.position);
            return;
        }
    }

    void Splash(DVector3 position) {
        foreach(var hit in World.current.FindEntitiesWithinRadius(position, splashRadius, entity.team)) {
            var dist = World.current.map.Distance(position, hit.position);
            hit.Damage(splashRadius * (dist / splashRadius), entity);
        }
        entity.Destroy();
        World.current.eventListener.Animate(entity, "Impact");
    }

    public override uint Checksum() {
        return 0;
    }
}

}
