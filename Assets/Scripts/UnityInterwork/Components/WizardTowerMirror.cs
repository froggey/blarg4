using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class WizardTowerMirror: MonoBehaviour {
    public Game.WizardTower component;

    ListController ui_manager;

    ListItemController harvesterUiElement;
    ListItemController wizardUiElement;

    void Start() {
        ui_manager = Object.FindObjectOfType<ListController>();

        harvesterUiElement = ui_manager.AddUiElement(
            Resources.Load<Sprite>("Sprites/" + component.harvesterPrototype),
            component.harvesterPrototype,
            cancel => {});
        wizardUiElement = ui_manager.AddUiElement(
            Resources.Load<Sprite>("Sprites/" + component.wizardPrototype),
            component.wizardPrototype,
            cancel => {});
    }

    void Update() {
        harvesterUiElement.SetUiEnabled(component.harvester == null);
        harvesterUiElement.SetProgress(((float)component.harvesterBuildCooldown - (float)component.harvesterBuildCooldownRemaining) / (float)component.harvesterBuildCooldown);
        wizardUiElement.SetUiEnabled(component.wizard == null);
        wizardUiElement.SetProgress(((float)component.wizardBuildCooldown - (float)component.wizardBuildCooldownRemaining) / (float)component.wizardBuildCooldown);
    }

    void OnSelect() {
        harvesterUiElement.gameObject.SetActive(true);
        wizardUiElement.gameObject.SetActive(true);
    }

    void OnDeselect() {
        harvesterUiElement.gameObject.SetActive(false);
        wizardUiElement.gameObject.SetActive(false);
    }

    void OnDestroy() {
        if(harvesterUiElement != null) {
            Destroy(harvesterUiElement.gameObject);
        }
        if(wizardUiElement != null) {
            Destroy(wizardUiElement.gameObject);
        }
    }
}

}
