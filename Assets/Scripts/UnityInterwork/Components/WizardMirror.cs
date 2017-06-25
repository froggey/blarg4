using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class WizardMirror: MonoBehaviour {
    public Game.Wizard component;

    Testshit testshit = null;
    ListController ui_manager;

    ListItemController deployUiElement;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
        ui_manager = Object.FindObjectOfType<ListController>();

        deployUiElement = ui_manager.AddUiElement(
            Resources.Load<Sprite>("Sprites/" + component.towerPrototype),
            component.towerPrototype,
            cancel => {
                if(!cancel) {
                    testshit.BeginPlacement(component.towerPrototype,
                                            point => testshit.DeployCommand(GetComponent<EntityMirror>(), point),
                                            point => component.CheckBuildPlacement((Game.DVector3)point),
                                            () => {});
                }
            });
    }

    void Update() {
        deployUiElement.SetUiEnabled(component.tower == null);
        deployUiElement.SetProgress(((float)component.towerBuildCooldown - (float)component.towerBuildCooldownRemaining) / (float)component.towerBuildCooldown);
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
