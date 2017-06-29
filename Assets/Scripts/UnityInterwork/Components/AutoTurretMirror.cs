using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class AutoTurretMirror: MonoBehaviour {
    public Game.AutoTurret component;

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.magenta, (float)component.weaponRange);
    }
}

}
