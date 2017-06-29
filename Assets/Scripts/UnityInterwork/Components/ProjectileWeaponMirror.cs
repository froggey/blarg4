using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class ProjectileWeaponMirror: InterworkComponent {
    public Game.ProjectileWeapon component;

    void Start() {
    }

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.red, (float)component.Range());
    }
}

}
