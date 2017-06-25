using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class ColliderMirror: MonoBehaviour {
    public Game.Collider component;

    void Start() {
    }

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCylinder(transform.position, transform.position + Vector3.up * (float)component.height, (float)component.radius);
    }
}

}
