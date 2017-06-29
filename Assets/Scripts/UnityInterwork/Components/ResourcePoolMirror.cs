using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class ResourcePoolMirror: InterworkComponent {
    public Game.ResourcePool component;
    public float fill;

    void LateUpdate() {
        fill = (float)component.fill;
    }
}

}