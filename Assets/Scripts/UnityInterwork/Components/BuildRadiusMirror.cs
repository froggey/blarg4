using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class BuildRadiusMirror: InterworkComponent {
    public Game.BuildRadius component;

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.blue, (float)component.radius);
    }
}

}
