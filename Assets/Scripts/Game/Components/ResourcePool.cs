using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ResourcePool: Component {
    public readonly int resourceId;
    public DReal fill;

    public ResourcePool(Entity entity, ComponentPrototype proto): base(entity) {
        this.resourceId = World.current.resourceNameToId[proto.data["resource"]];
        this.fill = DReal.Parse(proto.data["initial"]);
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
