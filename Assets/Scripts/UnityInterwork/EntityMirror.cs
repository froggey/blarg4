using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class EntityMirror: MonoBehaviour {
    public Vector3 positionAdjust;
    public Game.Entity entity;

    public int team;
    public float angle;

    public GameObject healthBarPrefab = null;
    public GameObject resourceBarPrefab = null;

    public EntityMirror parent = null;
    EntityMirror[] children;

    Testshit testshit = null;

    int currentTeamColour = -1;

    // For interpolation.
    float interpolationTime;
    int nextTick;
    Vector3 currentPosition;
    Vector3 nextPosition;
    Vector3 currentRotation;
    Vector3 nextRotation;

    Rigidbody rb;

    void UpdateTeamColour() {
        if(currentTeamColour != entity.team) {
            foreach(var mr in GetComponentsInChildren<Renderer>()) {
                mr.material.SetColor("_TeamColor", testshit.TeamColour(entity.team));
            }
            currentTeamColour = entity.team;
        }
    }

    void Start() {
        if(entity == null) {
            return;
        }

        rb = GetComponent<Rigidbody>();

        testshit = Object.FindObjectOfType<Testshit>();

        if(parent == null) {
            children = new EntityMirror[9];
            for(var i = 0; i < 9; i += 1) {
                var x = i / 3 - 1;
                var z = i % 3 - 1;
                if(x == 0 && z == 0) {
                    children[i] = this;
                } else {
                    var go = Instantiate(gameObject);
                    var mirror = go.GetComponent<UnityInterwork.EntityMirror>();
                    mirror.entity = entity;
                    mirror.positionAdjust = new Vector3(x*1024, 0, z*1024);
                    mirror.parent = this;
                    children[i] = mirror;
                }
            }

            foreach(var comp in entity.components) {
                if(comp is Game.Factory) {
                    var m = gameObject.AddComponent<FactoryMirror>();
                    m.component = (Game.Factory)comp;
                } else if(comp is Game.ResourceSource) {
                    var m = gameObject.AddComponent<ResourceSourceMirror>();
                    m.component = (Game.ResourceSource)comp;
                } else if(comp is Game.ResourcePool) {
                    var m = gameObject.AddComponent<ResourcePoolMirror>();
                    m.component = (Game.ResourcePool)comp;
                } else if(comp is Game.Wizard) {
                    var m = gameObject.AddComponent<WizardMirror>();
                    m.component = (Game.Wizard)comp;
                } else if(comp is Game.Truck) {
                    var m = gameObject.AddComponent<TruckMirror>();
                    m.component = (Game.Truck)comp;
                } else if(comp is Game.Collider) {
                    var m = gameObject.AddComponent<ColliderMirror>();
                    m.component = (Game.Collider)comp;
                } else if(comp is Game.ProjectileWeapon) {
                    var m = gameObject.AddComponent<ProjectileWeaponMirror>();
                    m.component = (Game.ProjectileWeapon)comp;
                } else if(comp is Game.HitscanWeapon) {
                    var m = gameObject.AddComponent<HitscanWeaponMirror>();
                    m.component = (Game.HitscanWeapon)comp;
                } else if(comp is Game.Health) {
                    var m = gameObject.AddComponent<HealthMirror>();
                    m.healthBarPrefab = healthBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.Health)comp;
                } else if(comp is Game.ResourceHarvester) {
                    var m = gameObject.AddComponent<ResourceHarvesterMirror>();
                    m.resourceBarPrefab = resourceBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.ResourceHarvester)comp;
                } else if(comp is Game.PartialBuilding) {
                    var m = gameObject.AddComponent<PartialBuildingMirror>();
                    m.resourceBarPrefab = resourceBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.PartialBuilding)comp;
                    // Don't do the death animation.
                    rb = null;
                } else if(comp is Game.WizardTower) {
                    var m = gameObject.AddComponent<WizardTowerMirror>();
                    m.component = (Game.WizardTower)comp;
                } else if(comp is Game.BuildRadius) {
                    var m = gameObject.AddComponent<BuildRadiusMirror>();
                    m.component = (Game.BuildRadius)comp;
                } else if(comp is Game.BasicUnit) {
                    var m = gameObject.AddComponent<BasicUnitMirror>();
                    m.component = (Game.BasicUnit)comp;
                } else {
                    Logger.Log("Unmirrorable component {0}", comp);
                }
            }
        } else {
            // eugh
            foreach(var comp in entity.components) {
                if(comp is Game.Health) {
                    var m = gameObject.AddComponent<HealthMirror>();
                    m.healthBarPrefab = healthBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.Health)comp;
                } else if(comp is Game.ResourceHarvester) {
                    var m = gameObject.AddComponent<ResourceHarvesterMirror>();
                    m.resourceBarPrefab = resourceBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.ResourceHarvester)comp;
                } else if(comp is Game.PartialBuilding) {
                    var m = gameObject.AddComponent<PartialBuildingMirror>();
                    m.resourceBarPrefab = resourceBarPrefab;
                    m.canvasTransform = Object.FindObjectOfType<PlayerInterface>().screenCanvas;
                    m.component = (Game.PartialBuilding)comp;
                    // Don't do the death animation.
                    rb = null;
                }
            }
        }

        UpdateTeamColour();
        transform.position = (Vector3)entity.position;
        transform.eulerAngles = new Vector3(0, (float)entity.rotation, 0);

        interpolationTime = 0;
        nextTick = Game.World.current.currentTick;
        currentPosition = transform.position;
        nextPosition = currentPosition;
        currentRotation = new Vector3(0, (float)entity.rotation, 0);
        nextRotation = currentRotation;
    }

    void Update() {
        if(entity == null) {
            return;
        }
        UpdateTeamColour();

        interpolationTime += Time.deltaTime;
        if(interpolationTime > (float)Game.World.deltaTime) {
            interpolationTime = (float)Game.World.deltaTime;
        }

        if(nextTick != Game.World.current.currentTick) {
            interpolationTime = 0;
            currentPosition = nextPosition;
            currentRotation = nextRotation;
            nextPosition = (Vector3)entity.position;
            nextRotation = new Vector3(0, (float)entity.rotation, 0);
            nextTick = Game.World.current.currentTick;
        }

        var lerp = interpolationTime / (float)Game.World.deltaTime;
        transform.position = Vector3.Lerp(currentPosition, nextPosition, lerp) + positionAdjust;
        transform.eulerAngles = Vector3.Lerp(currentRotation, nextRotation, lerp);

        if(!testshit.enableInterpolation) {
            transform.position = nextPosition + positionAdjust;
            transform.eulerAngles = nextRotation;
        }

        team = entity.team;
        angle = (float)entity.rotation;
    }

    void OnSelect() {
        if(parent == null) {
            for(var i = 0; i < 9; i += 1) {
                if(children[i] == this) {
                    continue;
                }
                children[i].gameObject.SendMessage("OnSelect");
            }
        }
    }

    void OnDeselect() {
        if(parent == null) {
            for(var i = 0; i < 9; i += 1) {
                if(children[i] == this) {
                    continue;
                }
                children[i].gameObject.SendMessage("OnDeselect");
            }
        }
    }

    public void Animate(string animation) {
        for(var i = 0; i < 9; i += 1) {
            children[i].gameObject.SendMessage("Animation" + animation, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void Destroyed() {
        if(parent == null) {
            for(var i = 0; i < 9; i += 1) {
                if(children[i] == this) {
                    continue;
                }
                children[i].Destroyed();
            }
        }
        if(rb == null) {
            Destroy(gameObject);
        } else {
            entity = null;
            foreach(var comp in GetComponentsInChildren<InterworkComponent>()) {
                Destroy(comp);
            }
            foreach(var comp in GetComponentsInChildren<Selectable>()) {
                Destroy(comp);
            }
            foreach(var comp in GetComponentsInChildren<UnityEngine.Collider>()) {
                Destroy(comp);
            }
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * 20, ForceMode.VelocityChange);
            rb.angularVelocity = Random.insideUnitSphere * 40;
            Destroy(gameObject, 10);
        }
    }
}

}
