using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class Wizard: Component {
    public string towerPrototype;

    public Entity tower;

    public DReal towerBuildCooldown;
    public DReal towerBuildCooldownRemaining;

    public Wizard(Entity entity, ComponentPrototype proto): base(entity) {
        towerPrototype = proto.data["tower"];
        towerBuildCooldown = DReal.Parse(World.current.entityPrototypes[towerPrototype].data["buildTime"]);
        towerBuildCooldownRemaining = towerBuildCooldown;
        tower = null;
    }

    public override void OnTick() {
        if(towerBuildCooldownRemaining > 0) {
            towerBuildCooldownRemaining -= World.deltaTime;
        }
    }

    public bool CheckBuildPlacement(DVector3 position) {
        // Eugh. Poke around in the prototype for the collider (if any).
        var radius = (DReal)0;
        var proto = World.current.entityPrototypes[towerPrototype];
        foreach(var cproto in proto.components) {
            if(cproto.kind == "Collider") {
                radius = DReal.Parse(cproto.data["radius"]);
            }
        }

        if(World.current.FindEntitiesWithinRadius(position, radius * 2).Any()) {
            return false;
        }

        // Must be with a build radius.
        foreach(var ent in World.current.entities.Where(e => e.team == entity.team)) {
            var br = ent.GetComponent<BuildRadius>();
            if(br != null && br.Contains(position, radius)) {
                return true;
            }
        }

        return false;
    }

    public override void DeployCommand(DVector3 position) {
        if(tower != null && tower.isAlive) {
            return;
        }
        if(towerBuildCooldownRemaining > 0) {
            return;
        }
        if(!CheckBuildPlacement(position)) {
            return;
        }
        // TODO: Respect the tower's build mode.
        tower = World.current.Instantiate(towerPrototype,
                                          entity.team,
                                          position);
        towerBuildCooldownRemaining = towerBuildCooldown;
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
