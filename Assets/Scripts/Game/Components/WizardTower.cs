using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class WizardTower: Component {
    public Entity harvester = null;
    public Entity wizard = null;
    public string harvesterPrototype;
    public DReal harvesterBuildCooldown;
    public DReal harvesterBuildCooldownRemaining;
    public string wizardPrototype;
    public DReal wizardBuildCooldown;
    public DReal wizardBuildCooldownRemaining;

    public WizardTower(Entity entity, ComponentPrototype proto): base(entity) {
        harvesterPrototype = proto.data["harvester"];
        harvesterBuildCooldown = DReal.Parse(World.current.entityPrototypes[harvesterPrototype].data["buildTime"]);
        wizardPrototype = proto.data["wizard"];
        wizardBuildCooldown = DReal.Parse(World.current.entityPrototypes[harvesterPrototype].data["buildTime"]);
        wizardBuildCooldownRemaining = wizardBuildCooldown;
    }

    public override void OnCreate() {
        wizard = World.current.entities
            .FirstOrDefault(e => {
                    return e.team == entity.team && e.modelName == wizardPrototype;
                });
    }

    public override void OnTick() {
        if(harvesterBuildCooldownRemaining > 0) {
            harvesterBuildCooldownRemaining -= World.deltaTime;
        }
        if(harvester != null && !harvester.isAlive) {
            harvester = null;
        }
        if(harvester == null && harvesterBuildCooldownRemaining <= 0) {
            // TODO: Respect the harvester's build mode.
            harvester = World.current.Instantiate(harvesterPrototype,
                                                  entity.team,
                                                  entity.RandomSpawnPosition(10,20));
            harvesterBuildCooldownRemaining = harvesterBuildCooldown;
        }
        if(wizardBuildCooldownRemaining > 0) {
            wizardBuildCooldownRemaining -= World.deltaTime;
        }
        if(wizard != null && !wizard.isAlive) {
            wizard = null;
        }
        if(wizard == null && wizardBuildCooldownRemaining <= 0) {
            // TODO: Respect the wizard's build mode.
            wizard = World.current.Instantiate(wizardPrototype,
                                               entity.team,
                                               entity.RandomSpawnPosition(10,20));
            wizardBuildCooldownRemaining = wizardBuildCooldown;
        }
    }

    public override void OnDestroy() {
        if(harvester != null) {
            harvester.Destroy();
        }
    }

    public override uint Checksum() {
        uint checksum = 0;
        if(harvester != null) {
            checksum ^= (uint)harvester.eid;
        }
        return checksum;
    }
}

}
