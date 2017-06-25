using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Collider: Component {
    public DReal height;
    public DReal radius;
    public bool fixedPosition;

    public Collider(Entity entity, ComponentPrototype proto): base(entity) {
        height = DReal.Parse(proto.data["height"]);
        radius = DReal.Parse(proto.data["radius"]);
        fixedPosition = proto.data["fixedPosition"] == "true";
    }

    public override void OnCreate() {
        World.current.RegisterCollider(this);
    }

    public override void OnCollision(Entity other) {
        if(fixedPosition) {
            return;
        }
        var other_collider = other.GetComponent<Collider>();
        var max_dist = radius + other_collider.radius;
        var max_dist_sqr = max_dist * max_dist;
        var sqr_dist = World.current.map.DistanceSqr(new DVector3(other.position.x, 0, other.position.z),
                                                     new DVector3(entity.position.x, 0, entity.position.z));
        var punt_power = max_dist_sqr / (max_dist_sqr - sqr_dist) / 2;
        var dir = World.current.map.Direction(new DVector3(other.position.x, 0, other.position.z),
                                              new DVector3(entity.position.x, 0, entity.position.z));
        entity.position += dir * punt_power;
    }

    public override void OnDestroy() {
        World.current.UnregisterCollider(this);
    }

    public override uint Checksum() {
        return 0;
    }
}

}
