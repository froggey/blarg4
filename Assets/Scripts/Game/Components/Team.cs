using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Team: Component {
    public Team(Entity entity): base(entity) {
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
