using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class BuildRadius: Component {
    public DReal radius;

    public BuildRadius(Entity entity, ComponentPrototype proto): base(entity) {
        radius = DReal.Parse(proto.data["radius"]);
    }

    public bool Contains(DVector3 point, DReal thing_radius) {
        var dist = World.current.map.Distance(point, entity.position);
        return dist < radius + thing_radius;
    }

    public override uint Checksum() {
        return radius.Checksum();
    }
}

}
