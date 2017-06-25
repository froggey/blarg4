using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Health: Component {
    public DReal max;
    public DReal current;

    public Dictionary<Entity, DReal> damageDealers = new Dictionary<Entity, DReal>();

    public Health(Entity entity, ComponentPrototype proto): base(entity) {
        max = DReal.Parse(proto.data["max"]);
        current = max;
    }

    public override void OnTick() {
        if(current <= 0) {
            World.current.eventListener.Animate(entity, "Death");
            entity.Destroy();
        }
    }

    public override uint Checksum() {
        return max.Checksum() ^ current.Checksum();
    }
}

}
