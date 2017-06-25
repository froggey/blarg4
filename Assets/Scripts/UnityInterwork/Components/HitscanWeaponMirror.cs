using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class HitscanWeaponMirror: MonoBehaviour {
    public Game.HitscanWeapon component;

    void Start() {
    }

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.red, (float)component.Range());
    }
}

}
