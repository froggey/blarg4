using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class ResourcePoolMirror: MonoBehaviour {
    public Game.ResourcePool component;
    public float fill;

    void LateUpdate() {
        fill = (float)component.fill;
    }
}

}