using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ProjectileWeapon: Component, IWeapon {
    public string projectile;

    public DReal range;
    public DReal fireRate;

    public DReal fireTime;

    public Collider collider;

    public ProjectileWeapon(Entity entity, ComponentPrototype proto): base(entity) {
        projectile = proto.data["projectile"];
        range = DReal.Parse(proto.data["range"]);
        fireRate = DReal.Parse(proto.data["fireRate"]);

        fireTime = 0;
    }

    public override void OnCreate() {
        collider = entity.GetComponent<Collider>();
    }

    public override void OnTick() {
        if(fireTime > 0) {
            fireTime -= World.deltaTime;
        }
    }

    public DReal Range() {
        return range;
    }

    public void FireAt(Entity target) {
        if(fireTime > 0) {
            return;
        }

        var dist = entity.Range(target);
        if(dist > range) {
            return;
        }

        var target_position = target.position;

        // Aim for the middle.
        var target_collider = target.GetComponent<Collider>();
        if(target_collider != null) {
            target_position += new DVector3(0, target_collider.height/2, 0);
        }

        var projectile_spawn = entity.position;
        if(collider != null) {
            // Spawn from front middle of the collider.
            projectile_spawn += new DVector3(0, collider.height/2, 0);
            projectile_spawn += entity.faceDirection * collider.radius;
        }
        // Offset the origin slightly to make it less repetative.
        projectile_spawn += new DVector3(World.current.RandomRange(-1,1),
                                         World.current.RandomRange(-1,1),
                                         World.current.RandomRange(-1,1));

        var dir = World.current.map.Direction(projectile_spawn, target_position);

        fireTime = fireRate;
        var ent = World.current.Instantiate(projectile, entity.team, projectile_spawn);
        var proj = ent.GetComponent<Projectile>();
        if(proj != null) {
            proj.spawner = entity;
            proj.dir = dir;
        }
        World.current.eventListener.Animate(entity, "Fire");
    }

    public override uint Checksum() {
        return 0;
    }
}

}
