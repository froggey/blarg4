using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class FactoryMirror: InterworkComponent {
    public Game.Factory component;
    public string[] buildables;

    Testshit testshit = null;
    ListController ui_manager;

    ListItemController[] ui_elements;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
        buildables = component.buildables;

        ui_manager = Object.FindObjectOfType<ListController>();

        ui_elements = new ListItemController[buildables.Length];
        for(var i = 0; i < buildables.Length; i += 1) {
            var id = i; // closure
            ui_elements[i] = ui_manager.AddUiElement(Resources.Load<Sprite>("Sprites/" + buildables[i]),
                                                     buildables[i],
                                                     (cancel) => DoBuild(id, cancel));
        }
    }

    void Update() {
        if(component.buildInProgress) {
            foreach(var elt in ui_elements) {
                elt.SetUiEnabled(false);
                elt.SetProgress(0);
            }
            ui_elements[component.buildingWhat].SetUiEnabled(true);
            ui_elements[component.buildingWhat].SetProgress(Mathf.Clamp01(((float)component.totalTime - (float)component.remainingTime) / (float)component.totalTime));
        } else {
            for(var i = 0; i < buildables.Length; i += 1) {
                var ui_elt = ui_elements[i];
                ui_elt.SetUiEnabled(component.HaveEnoughResources(i));
                ui_elt.SetProgress(0);
            }
        }
    }

    void OnSelect() {
        foreach(var elt in ui_elements) {
            elt.gameObject.SetActive(true);
        }
    }

    void OnDeselect() {
        foreach(var elt in ui_elements) {
            elt.gameObject.SetActive(false);
        }
    }

    void OnDestroy() {
        foreach(var elt in ui_elements) {
            if(elt != null) {
                Destroy(elt.gameObject);
            }
        }
    }

    void DoBuild(int id, bool cancel) {
        if(cancel) {
            testshit.StopCommand(GetComponent<EntityMirror>());
            return;
        }
        if(component.buildInProgress && component.currentMode == Game.BuildMode.BUILD_THEN_PLACE && component.remainingTime <= 0) {
            testshit.BeginPlacement(buildables[component.buildingWhat],
                                    point => testshit.DeployCommand(GetComponent<EntityMirror>(), point),
                                    point => component.CheckBuildPlacement(component.buildingWhat, (Game.DVector3)point),
                                    () => {});
            return;
        }

        var mode = Game.World.current.entityPrototypes[buildables[id]].BuildMode();
        if(mode == Game.BuildMode.BUILD_IN_PLACE ||
           mode == Game.BuildMode.BUILD_IMMEDIATE) {
            testshit.BeginPlacement(buildables[id],
                                    point => testshit.BuildCommand(GetComponent<EntityMirror>(), id, point),
                                    point => component.CheckBuildPlacement(id, (Game.DVector3)point) && component.HaveEnoughResources(id),
                                    () => {});
        } else {
            testshit.BuildCommand(GetComponent<EntityMirror>(), id, Vector3.zero);
        }
    }

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.green, (float)component.spawnMin);
        DebugExtension.DrawCircle(transform.position, Color.green, (float)component.spawnMax);
    }
}

}
