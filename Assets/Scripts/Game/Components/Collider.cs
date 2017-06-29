using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Collider: Component {
    public DReal height;
    public DReal radius;
    public bool fixedPosition;
    public bool pushy;

    public Collider(Entity entity, ComponentPrototype proto): base(entity) {
        height = DReal.Parse(proto.data["height"]);
        radius = DReal.Parse(proto.data["radius"]);
        fixedPosition = proto.data["fixedPosition"] == "true";
        pushy = proto.data["pushy"] == "true";
    }

    public override void OnCreate() {
        World.current.RegisterCollider(this);
    }

    public override void OnCollision(Entity other) {
        if(fixedPosition) {
            return;
        }
        var other_collider = other.GetComponent<Collider>();
        if(!other_collider.pushy) {
            return;
        }
        var max_dist = radius + other_collider.radius;
        var dist = World.current.map.Distance(new DVector3(other.position.x, 0, other.position.z),
                                              new DVector3(entity.position.x, 0, entity.position.z));
        var dir = World.current.map.Direction(new DVector3(other.position.x, 0, other.position.z),
                                              new DVector3(entity.position.x, 0, entity.position.z));
        entity.position += dir * (max_dist - dist) / 2;
    }

    public override void OnDestroy() {
        World.current.UnregisterCollider(this);
    }

    public override uint Checksum() {
        return 0;
    }
}

}
