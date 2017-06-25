using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Mine: Component {
    ResourcePool pool = null;

    public ResourceSource source;
    DReal rate;

    public Mine(Entity entity, ComponentPrototype proto): base(entity) {
        this.rate = DReal.Parse(proto.data["rate"]);

        var resource = World.current.resourceNameToId[proto.data["resource"]];

        var team_ent = World.current.entities.First(e => e.team == entity.team && e.GetComponents<ResourcePool>().Any(p => p.resourceId == resource));
        pool = team_ent.GetComponents<ResourcePool>().First(p => p.resourceId == resource);
    }

    public override void OnCreate() {
        if(source.occupied) {
            entity.Destroy();
        }
        source.occupied = true;
    }

    public override void OnTick() {
        pool.fill += source.Take(rate * World.deltaTime);
    }

    public override void OnDestroy() {
        source.occupied = false;
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
