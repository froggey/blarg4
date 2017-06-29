using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class BasicUnitMirror: InterworkComponent {
    public Game.BasicUnit component;

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.magenta, (float)component.weaponRange);
    }
}

}
