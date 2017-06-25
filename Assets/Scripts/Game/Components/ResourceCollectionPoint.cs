using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ResourceCollectionPoint: Component {
    ResourcePool pool = null;

    public readonly int resourceId;

    public ResourceCollectionPoint(Entity entity, int resourceId): base(entity) {
        this.resourceId = resourceId;

        var team_ent = World.current.entities.First(e => e.team == entity.team && e.GetComponents<ResourcePool>().Any(p => p.resourceId == resourceId));
        pool = team_ent.GetComponents<ResourcePool>().First(p => p.resourceId == resourceId);
    }

    public ResourceCollectionPoint(Entity entity, ComponentPrototype proto): base(entity) {
        this.resourceId = World.current.resourceNameToId[proto.data["resource"]];

        var team_ent = World.current.entities.First(e => e.team == entity.team && e.GetComponents<ResourcePool>().Any(p => p.resourceId == resourceId));
        pool = team_ent.GetComponents<ResourcePool>().First(p => p.resourceId == resourceId);
    }

    public void Receive(DReal count) {
        pool.fill += count;
    }

    public override uint Checksum() {
        return (uint)resourceId;
    }
}

}
