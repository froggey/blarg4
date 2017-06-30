using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityInterwork {

class FactoryMirror: InterworkComponent {
    public class Buildable {
        public string name;
        public int id;
        public ListItemController uiElement;
    }

    public Game.Factory component;
    public List<Buildable> buildables;

    Testshit testshit = null;
    ListController ui_manager;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
        buildables = new List<Buildable>();

        ui_manager = Object.FindObjectOfType<ListController>();

        for(var i = 0; i < component.buildables.Length; i += 1) {
            if(component.buildables[i] == null) {
                continue;
            }
            var name = component.buildables[i];
            var buildable = new Buildable();
            buildable.name = name;
            buildable.id = i;
            buildable.uiElement = ui_manager.AddUiElement(Resources.Load<Sprite>("Sprites/" + name),
                                                          name,
                                                          (cancel) => DoBuild(buildable, cancel));
            buildables.Add(buildable);
        }
    }

    void Update() {
        if(component.buildInProgress) {
            foreach(var elt in buildables) {
                elt.uiElement.SetUiEnabled(false);
                elt.uiElement.SetProgress(0);
            }
            var active_elt = buildables.Single(e => e.id == component.buildingWhat);
            active_elt.uiElement.SetUiEnabled(true);
            active_elt.uiElement.SetProgress(Mathf.Clamp01(((float)component.totalTime - (float)component.remainingTime) / (float)component.totalTime));
        } else {
            foreach(var elt in buildables) {
                elt.uiElement.SetUiEnabled(component.HaveEnoughResources(elt.id));
                elt.uiElement.SetProgress(0);
            }
        }
    }

    void OnSelect() {
        foreach(var elt in buildables) {
            elt.uiElement.gameObject.SetActive(true);
        }
    }

    void OnDeselect() {
        foreach(var elt in buildables) {
            elt.uiElement.gameObject.SetActive(false);
        }
    }

    void OnDestroy() {
        foreach(var elt in buildables) {
            if(elt.uiElement != null) {
                Destroy(elt.uiElement.gameObject);
            }
        }
    }

    void DoBuild(Buildable elt, bool cancel) {
        if(cancel) {
            testshit.StopCommand(GetComponent<EntityMirror>());
            return;
        }
        if(component.buildInProgress && component.currentMode == Game.BuildMode.BUILD_THEN_PLACE && component.remainingTime <= 0) {
            if(elt.id != component.buildingWhat) {
                return;
            }
            testshit.BeginPlacement(elt.name,
                                    point => testshit.DeployCommand(GetComponent<EntityMirror>(), point),
                                    point => component.CheckBuildPlacement(component.buildingWhat, (Game.DVector3)point),
                                    () => {});
            return;
        }

        var mode = Game.World.current.entityPrototypes[elt.name].BuildMode();
        if(mode == Game.BuildMode.BUILD_IN_PLACE ||
           mode == Game.BuildMode.BUILD_IMMEDIATE) {
            testshit.BeginPlacement(elt.name,
                                    point => testshit.BuildCommand(GetComponent<EntityMirror>(), elt.id, point),
                                    point => component.CheckBuildPlacement(elt.id, (Game.DVector3)point) && component.HaveEnoughResources(elt.id),
                                    () => {});
        } else {
            testshit.BuildCommand(GetComponent<EntityMirror>(), elt.id, Vector3.zero);
        }
    }

    void OnDrawGizmosSelected() {
        DebugExtension.DrawCircle(transform.position, Color.green, (float)component.spawnMin);
        DebugExtension.DrawCircle(transform.position, Color.green, (float)component.spawnMax);
    }
}

}
