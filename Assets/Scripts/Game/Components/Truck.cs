using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Truck: Component {
    public string deployPrototype;

    public Truck(Entity entity, ComponentPrototype proto): base(entity) {
        deployPrototype = proto.data["deploy"];
    }

    public override void DeployCommand(DVector3 position) {
        World.current.Instantiate(deployPrototype,
                                  entity.team,
                                  entity.position);
        entity.Destroy();
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
