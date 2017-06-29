using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class ResourceSourceMirror: InterworkComponent {
    public Game.ResourceSource component;
    public float remainingCount;
    public bool occupied;

    void LateUpdate() {
        remainingCount = (float)component.remainingCount;
        occupied = component.occupied;
    }
}

}