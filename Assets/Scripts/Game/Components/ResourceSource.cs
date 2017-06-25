using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ResourceSource: Component {
    public readonly int resourceId;
    public DReal remainingCount;
    public bool occupied;

    public ResourceSource(Entity entity, ComponentPrototype proto): base(entity) {
        this.resourceId = World.current.resourceNameToId[proto.data["resource"]];
        this.remainingCount = DReal.Parse(proto.data["initial"]);
        this.occupied = false;
    }

    public DReal Take(DReal count) {
        var n = DReal.Min(remainingCount, count);
        remainingCount -= n;
        return n;
    }

    public bool Depleted() {
        return remainingCount <= 0;
    }

    public override uint Checksum() {
        return (uint)resourceId ^ remainingCount.Checksum() ^ (uint)(occupied ? 1 : 0);
    }
}

}
