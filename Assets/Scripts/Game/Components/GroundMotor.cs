using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class GroundMotor: Component, IMotor {
    public DReal moveSpeed;

    List<Game.DVector3> currentPath;

    public GroundMotor(Entity entity, ComponentPrototype proto): base(entity) {
        moveSpeed = DReal.Parse(proto.data["speed"]);
    }

    public override void OnCreate() {
        entity.position = new DVector3(entity.position.x,
                                       World.current.map.Height(entity.position),
                                       entity.position.z);
    }

    public override void OnTick() {
        var remaining_movement = moveSpeed * World.deltaTime;
        while(remaining_movement > 0 && currentPath != null && currentPath.Count != 0) {
            // If there is something in the way, then ignore this waypoint.
            if(currentPath.Count > 1 && World.current.FindEntitiesWithinRadius(new DVector3(currentPath[0].x, 0, currentPath[0].z), 0).Where(e => e != entity).Any()) {
                currentPath.RemoveAt(0);
                continue;
            }
            var dist = World.current.map.Distance(new DVector3(entity.position.x, 0, entity.position.z),
                                                  new DVector3(currentPath[0].x, 0, currentPath[0].z));
            var dir = World.current.map.Direction(new DVector3(entity.position.x, 0, entity.position.z),
                                                  new DVector3(currentPath[0].x, 0, currentPath[0].z));
            var px = (DReal)0;
            var pz = (DReal)0;

            if(dist > remaining_movement) {
                var motion = dir * remaining_movement;
                px = DReal.Repeat(entity.position.x + motion.x,
                                  World.current.map.width);
                pz = DReal.Repeat(entity.position.z + motion.z,
                                  World.current.map.depth);
                remaining_movement = 0;
            } else {
                px = currentPath[0].x;
                pz = currentPath[0].z;
                currentPath.RemoveAt(0);
                remaining_movement -= dist;
            }
            var new_height = World.current.map.Height(new DVector3(px, 0, pz));
            entity.position = new DVector3(px, new_height, pz);
            entity.faceDirection = dir;
        }

        entity.position = new DVector3(entity.position.x,
                                       World.current.map.Height(entity.position),
                                       entity.position.z);
    }

    public bool Reachable(DVector3 position) {
        return World.current.navigation.Reachability(entity.position) == World.current.navigation.Reachability(position);
    }

    public bool MoveTo(DVector3 position) {
        if(currentPath != null && currentPath.Count != 0) {
            // Try to avoid recomputing path.
            var dist = World.current.map.Distance(position, currentPath[currentPath.Count-1]);
            if(dist < 1) {
                return true;
            }
        }
        var newPath = World.current.navigation.BuildPath(entity.position, position);
        if(newPath == null) {
            return false;
        }
        currentPath = newPath;
        return true;
    }

    public void Stop() {
        currentPath = null;
    }

    public override uint Checksum() {
        uint checksum = moveSpeed.Checksum();
        if(currentPath != null) {
            foreach(var node in currentPath) {
                checksum ^= node.Checksum();
            }
        }
        return checksum;
    }
}

}