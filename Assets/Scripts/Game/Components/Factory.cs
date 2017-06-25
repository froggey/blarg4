using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Factory: Component {
    public string[] buildables;
    Entity teamEnt;
    BuildRadius localBuildRadius;
    Collider localCollider;

    public DReal spawnMin {
        get { return localCollider == null ? 0 : localCollider.radius; }
    }
    public DReal spawnMax {
        get { return localBuildRadius.radius; }
    }

    public bool buildInProgress;
    public BuildMode currentMode;
    public DReal totalTime;
    public DReal remainingTime;
    public int buildingWhat;
    DVector3 buildingPosition;
    PartialBuilding inPlaceConstruction;

    public Factory(Entity entity, ComponentPrototype proto): base(entity) {
        // Eugh.
        buildables = new string[10];
        proto.data.TryGetValue("build0", out buildables[0]);
        proto.data.TryGetValue("build1", out buildables[1]);
        proto.data.TryGetValue("build2", out buildables[2]);
        proto.data.TryGetValue("build3", out buildables[3]);
        proto.data.TryGetValue("build4", out buildables[4]);
        proto.data.TryGetValue("build5", out buildables[5]);
        proto.data.TryGetValue("build6", out buildables[6]);
        proto.data.TryGetValue("build7", out buildables[7]);
        proto.data.TryGetValue("build8", out buildables[8]);
        proto.data.TryGetValue("build9", out buildables[9]);
        buildables = buildables.Where(x => x != null).ToArray();
    }

    public override void OnCreate() {
        teamEnt = World.current.entities.First(e => e.team == entity.team && e.GetComponent<Team>() != null);

        localBuildRadius = entity.GetComponent<BuildRadius>();
        localCollider = entity.GetComponent<Collider>();
    }

    ResourcePool GetResourcePool(int resourceId) {
        return teamEnt.GetComponents<ResourcePool>().Single(p => p.resourceId == resourceId);
    }

    public bool HaveEnoughResources(int id) {
        var prefab = World.current.entityPrototypes[buildables[id]];
        var needed_resources = World.current.ParseResources(prefab.resources);
        foreach(var resource in needed_resources.resources) {
            if(GetResourcePool(resource.Key).fill < resource.Value) {
                return false;
            }
        }
        return true;
    }

    public bool CheckBuildPlacement(int id, DVector3 position) {
        // Eugh. Poke around in the prototype for the collider (if any).
        var radius = (DReal)0;
        var proto = World.current.entityPrototypes[buildables[id]];
        foreach(var cproto in proto.components) {
            if(cproto.kind == "Collider") {
                radius = DReal.Parse(cproto.data["radius"]);
            }
        }

        if(World.current.FindEntitiesWithinRadius(position, radius * 2).Any()) {
            return false;
        }

        // Must be within a build radius.
        foreach(var ent in World.current.entities.Where(e => e.team == entity.team)) {
            var br = ent.GetComponent<BuildRadius>();
            if(br != null && br.Contains(position, radius)) {
                return true;
            }
        }

        return false;
    }

    void ConsumeResources(int id, bool refund) {
        var prefab = World.current.entityPrototypes[buildables[id]];
        var needed_resources = World.current.ParseResources(prefab.resources);
        foreach(var resource in needed_resources.resources) {
            if(refund) {
                GetResourcePool(resource.Key).fill += resource.Value;
            } else {
                GetResourcePool(resource.Key).fill -= resource.Value;
            }
        }
    }

    public override void OnTick() {
        if(!buildInProgress) {
            return;
        }
        switch(currentMode) {
        case BuildMode.SPAWN:
            remainingTime -= World.deltaTime;
            if(remainingTime <= 0) {
                World.current.Instantiate(buildables[buildingWhat],
                                          entity.team,
                                          entity.RandomSpawnPosition(spawnMin, spawnMax));
                World.current.eventListener.Animate(entity, "Spawn");
                buildInProgress = false;
            }
            break;
        case BuildMode.SPAWN_IMMEDIATE:
            remainingTime -= World.deltaTime;
            if(remainingTime <= 0) {
                buildInProgress = false;
            }
            break;
        case BuildMode.BUILD_THEN_PLACE:
            if(remainingTime > 0) {
                remainingTime -= World.deltaTime;
            }
            break;
        case BuildMode.BUILD_IN_PLACE:
            remainingTime -= World.deltaTime;
            if(!inPlaceConstruction.entity.isAlive) {
                buildInProgress = false;
                break;
            }
            inPlaceConstruction.buildProgress = (totalTime - remainingTime) / totalTime;
            if(remainingTime <= 0) {
                // TODO: Inherit health.
                inPlaceConstruction.entity.Destroy();
                World.current.Instantiate(buildables[buildingWhat],
                                          entity.team,
                                          buildingPosition);
                buildInProgress = false;
            }
            break;
        case BuildMode.BUILD_IMMEDIATE:
            remainingTime -= World.deltaTime;
            if(remainingTime <= 0) {
                buildInProgress = false;
            }
            break;
        }
    }

    public override void OnDestroy() {
        if(buildInProgress) {
            ConsumeResources(buildingWhat, true);
            if(currentMode == BuildMode.BUILD_IN_PLACE) {
                inPlaceConstruction.entity.Destroy();
            }
        }
    }

    public override void DeployCommand(DVector3 position) {
        if(!buildInProgress) {
            return;
        }
        if(currentMode != BuildMode.BUILD_THEN_PLACE) {
            return;
        }
        if(remainingTime > 0) {
            return;
        }

        position = new DVector3(position.x,
                                World.current.map.Height(position),
                                position.z);

        if(!CheckBuildPlacement(buildingWhat, position)) {
            return;
        }

        World.current.Instantiate(buildables[buildingWhat],
                                  entity.team,
                                  position);
        buildInProgress = false;
    }

    public override void BuildCommand(int id, DVector3 position) {
        if(buildInProgress) {
            return;
        }

        position = new DVector3(position.x,
                                World.current.map.Height(position),
                                position.z);

        currentMode = World.current.entityPrototypes[buildables[id]].BuildMode();
        if(currentMode == BuildMode.BUILD_IN_PLACE || currentMode == BuildMode.BUILD_IMMEDIATE) {
            if(!CheckBuildPlacement(id, position)) {
                return;
            }
        }
        // TODO: Check placement for BUILD_foo here.
        if(!HaveEnoughResources(id)) {
            return;
        }

        ConsumeResources(id, false);
        buildInProgress = true;
        totalTime = DReal.Parse(World.current.entityPrototypes[buildables[id]].data["buildTime"]);
        remainingTime = totalTime;
        buildingWhat = id;
        buildingPosition = position;
        switch(currentMode) {
        case BuildMode.SPAWN:
            break;
        case BuildMode.SPAWN_IMMEDIATE:
            World.current.Instantiate(buildables[buildingWhat],
                                      entity.team,
                                      entity.RandomSpawnPosition(spawnMin, spawnMax));
            World.current.eventListener.Animate(entity, "Spawn");
            break;
        case BuildMode.BUILD_THEN_PLACE:
            break;
        case BuildMode.BUILD_IN_PLACE:
            var proto = World.current.entityPrototypes[buildables[id]];
            var ent = new Entity(entity.team, position, proto);
            // Only instantiate the collider and the health component, if any.
            foreach(var cproto in proto.components) {
                if(cproto.kind == "Collider") {
                    ent.AddComponent(new Collider(ent, cproto));
                } else if(cproto.kind == "Health") {
                    ent.AddComponent(new Health(ent, cproto));
                }
            }
            inPlaceConstruction = new PartialBuilding(ent);
            ent.AddComponent(inPlaceConstruction);
            break;
        case BuildMode.BUILD_IMMEDIATE:
            World.current.Instantiate(buildables[buildingWhat],
                                      entity.team,
                                      buildingPosition);
            break;
        }
    }

    public override void StopCommand() {
        if(buildInProgress) {
            buildInProgress = false;
            ConsumeResources(buildingWhat, true);
            if(currentMode == BuildMode.BUILD_IN_PLACE) {
                inPlaceConstruction.entity.Destroy();
            }
        }
    }

    public override uint Checksum() {
        return 0;
    }
}

public class PartialBuilding: Component {
    Health health;
    DReal realMaxHealth;

    public DReal buildProgress;

    public PartialBuilding(Entity ent): base(ent) {}

    public override void OnCreate() {
        health = entity.GetComponent<Health>();
        if(health != null) {
            realMaxHealth = health.max;
            health.max = 1;
            health.current = 1;
        }
    }

    public override void OnTick() {
        if(health != null) {
            var missing_health = health.max - health.current;
            health.max = DReal.Max(1, realMaxHealth * buildProgress);
            health.current = health.max - missing_health;
        }
    }

    public override uint Checksum() {
        return buildProgress.Checksum();
    }
}

}
