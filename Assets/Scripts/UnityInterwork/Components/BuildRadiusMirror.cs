using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class BuildRadiusMirror: MonoBehaviour {
    public Game.BuildRadius component;

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.blue, (float)component.radius);
    }
}

}
