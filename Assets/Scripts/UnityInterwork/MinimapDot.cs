using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityInterwork {

class MinimapController {
    public float xoff;
    public float zoff;
}

class MinimapDot: MonoBehaviour {
    public Game.Entity entity;
    public Image image = null;

    public MinimapController controller = null;

    Testshit testshit = null;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
    }

    void Update() {
        if(entity == null) {
            return;
        }

        image.color = testshit.TeamColour(entity.team);
        // TODO: Interpolate positions.
        transform.localPosition = new Vector3(((Mathf.Repeat((float)entity.position.x + controller.xoff, 1024) / 1024 * 2 - 1) * 100) % 100,
                                              ((Mathf.Repeat((float)entity.position.z + controller.zoff, 1024) / 1024 * 2 - 1) * 100) % 100,
                                              0);
    }
}

}
