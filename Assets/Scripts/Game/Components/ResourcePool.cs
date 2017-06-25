using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ResourcePool: Component {
    public readonly int resourceId;
    public DReal fill;

    public ResourcePool(Entity entity, int resourceId, DReal initial): base(entity) {
        this.resourceId = resourceId;
        this.fill = initial;
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
