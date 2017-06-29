using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class BasicUnitMirror: MonoBehaviour {
    public Game.BasicUnit component;

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.magenta, (float)component.weaponRange);
    }
}

}
