using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class ResourceHarvester: Component {
    IMotor motor;

    public readonly int resourceId;
    public readonly DReal capacity;
    public readonly DReal fillRate;
    public DReal fill;

    public ResourceCollectionPoint collector = null;
    public ResourceSource resource = null;

    public ResourceHarvester(Entity entity, int resourceId, DReal capacity, DReal fillRate): base(entity) {
        this.motor = entity.GetComponent<IMotor>();
        this.resourceId = resourceId;
        this.capacity = capacity;
        this.fillRate = fillRate;
        this.fill = 0;
    }

    public ResourceHarvester(Entity entity, ComponentPrototype proto): base(entity) {
        this.motor = entity.GetComponent<IMotor>();
        resourceId = World.current.resourceNameToId[proto.data["resource"]];
        capacity = DReal.Parse(proto.data["capacity"]);
        fillRate = DReal.Parse(proto.data["rate"]);
        this.fill = 0;
    }

    bool is_idle = false;

    bool moving_to_collector = false;

    public override void OnTick() {
        if(collector != null && !collector.entity.isAlive) {
            collector = null;
        }
        if(resource != null && !resource.entity.isAlive) {
            resource = null;
        }
        if(resource != null && resource.occupied) {
            resource = null;
        }
        if(is_idle) {
            return;
        }
        if(moving_to_collector) {
            if(collector == null) {
                // Find the nearest collector.
                var ent = World.current.entities
                    .Where(e => e.team == entity.team && e.GetComponents<ResourceCollectionPoint>().Any(s => s.resourceId == resourceId))
                    .OrderBy(e => World.current.map.Distance(entity.position, e.position))
                    .FirstOrDefault();
                if(ent != null) {
                    collector = ent.GetComponents<ResourceCollectionPoint>().Where(s => s.resourceId == resourceId).FirstOrDefault();
                }
            }
            if(collector == null) {
                motor.Stop();
                is_idle = true;
                return;
            }
            var dist = World.current.map.Distance(new DVector3(entity.position.x, 0, entity.position.z),
                                                  new DVector3(collector.entity.position.x, 0, collector.entity.position.z));
            if(dist < 3) {
                motor.Stop();
                // At the dropoff, disgorge the tank contents.
                var transfered = DReal.Min(fill, fillRate * World.deltaTime);
                collector.Receive(transfered);
                fill -= transfered;
            } else {
                motor.MoveTo(collector.entity.position);
            }
            // If the tank is empty, move to the resource.
            if(fill <= 0) {
                moving_to_collector = false;
                motor.Stop();
            }
        } else {
            if(resource != null && resource.remainingCount <= 0) {
                resource = null;
            }
            if(resource == null) {
                // Find the nearest resource.
                var ent = World.current.entities
                    .Where(e => e.GetComponents<ResourceSource>().Any(s => s.resourceId == resourceId && s.remainingCount > 0 && !s.occupied))
                    .OrderBy(e => World.current.map.Distance(entity.position, e.position))
                    .FirstOrDefault();
                if(ent != null) {
                    resource = ent.GetComponents<ResourceSource>().Where(s => s.resourceId == resourceId && s.remainingCount > 0 && !s.occupied).FirstOrDefault();
                }
            }
            if(resource == null) {
                motor.Stop();
                moving_to_collector = true;
                return;
            }
            var dist = World.current.map.Distance(new DVector3(entity.position.x, 0, entity.position.z),
                                                  new DVector3(resource.entity.position.x, 0, resource.entity.position.z));
            if(dist < 3) {
                motor.Stop();
                // At the pickup, fill the tank.
                var transfered = DReal.Min(capacity - fill, fillRate * World.deltaTime);
                fill += resource.Take(transfered);
            } else {
                motor.MoveTo(resource.entity.position);
            }
            // If the tank is full, move to the collector.
            if(fill >= capacity) {
                moving_to_collector = true;
                motor.Stop();
            }
        }
    }

    public override void StopCommand() {
        is_idle = true;
        motor.Stop();
    }

    public override void MoveCommand(DVector3 position) {
        is_idle = true;
        motor.MoveTo(position);
    }

    public override void AttackCommand(Entity target) {
        if(!motor.Reachable(target.position)) {
            return;
        }
        if(target.team == entity.team) {
            foreach(var collector in target.GetComponents<ResourceCollectionPoint>()) {
                if(collector.resourceId == resourceId) {
                    this.collector = collector;
                    motor.Stop();
                    is_idle = false;
                    moving_to_collector = true;
                }
            }
        }
        foreach(var resource in target.GetComponents<ResourceSource>()) {
            if(resource.resourceId == resourceId && !resource.occupied) {
                this.resource = resource;
                motor.Stop();
                is_idle = false;
                moving_to_collector = false;
            }
        }
    }

    public override uint Checksum() {
        uint checksum = (uint)resourceId ^ capacity.Checksum() ^ fillRate.Checksum() ^ fill.Checksum();
        if(collector != null) {
            checksum ^= (uint)collector.entity.eid;
        }
        if(resource != null) {
            checksum ^= (uint)resource.entity.eid;
        }
        return checksum;
    }
}

}
