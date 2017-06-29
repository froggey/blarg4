using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class TruckMirror: InterworkComponent {
    public Game.Truck component;

    Testshit testshit = null;
    ListController ui_manager;

    ListItemController deployUiElement;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
        ui_manager = Object.FindObjectOfType<ListController>();

        deployUiElement = ui_manager.AddUiElement(
            Resources.Load<Sprite>("Sprites/" + component.deployPrototype),
            component.deployPrototype,
            cancel => {
                if(!cancel) {
                    testshit.DeployCommand(GetComponent<EntityMirror>(),
                                           // Truck deploys in place.
                                           Vector3.zero);
                }});
    }

    void OnSelect() {
        deployUiElement.gameObject.SetActive(true);
    }

    void OnDeselect() {
        deployUiElement.gameObject.SetActive(false);
    }

    void OnDestroy() {
        if(deployUiElement != null) {
            Destroy(deployUiElement.gameObject);
        }
    }
}

}
