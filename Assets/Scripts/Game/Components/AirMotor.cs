using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class AirMotor: Component, IMotor {
    public DReal moveSpeed;
    public bool nonstop;

    public DReal wobbleFrequency;
    public DReal wobbleAmplitude;

    public DReal floatOffset;
    public DReal verticalSpeed;

    DReal wobbleOffset;

    DVector3 target;

    public AirMotor(Entity entity, ComponentPrototype proto): base(entity) {
        moveSpeed = DReal.Parse(proto.data["speed"]);

        wobbleFrequency = DReal.Parse(proto.data["wobbleFrequency"]);
        wobbleAmplitude = DReal.Parse(proto.data["wobbleAmplitude"]);
        floatOffset = DReal.Parse(proto.data["floatOffset"]);
        verticalSpeed = DReal.Parse(proto.data["verticalSpeed"]);

        wobbleOffset = World.current.RandomValue() * DReal.PI;
    }

    public override void OnCreate() {
        target = entity.position;
    }

    public override void OnTick() {
        var p2d = new DVector3(entity.position.x, 0, entity.position.z);
        var targ = new Game.DVector3(target.x, 0, target.z);
        var dist = World.current.map.Distance(p2d, targ);
        var dir = World.current.map.Direction(p2d, targ);
        if(dist < moveSpeed * World.deltaTime) {
            entity.position = new DVector3(target.x, entity.position.y, target.z);
        } else {
            var d = p2d + dir * moveSpeed * World.deltaTime;
            entity.position = new DVector3(d.x, entity.position.y, d.z);
            entity.faceDirection = dir;
        }

        var terrainHeight = DReal.Max(0, World.current.map.Height(entity.position));
        var wobble = DReal.Sin((World.current.time + wobbleOffset) * wobbleFrequency) * wobbleAmplitude;
        var targetHeight = terrainHeight + floatOffset + wobble;

        var height_diff = DReal.Abs(targetHeight - entity.position.y);
        var newHeight = (DReal)0;
        if(targetHeight > entity.position.y) {
            newHeight = entity.position.y + DReal.Min(height_diff, verticalSpeed * World.deltaTime);
        } else {
            newHeight = entity.position.y - DReal.Min(height_diff, verticalSpeed * World.deltaTime);
        }

        entity.position = new DVector3(entity.position.x,
                                       DReal.Max(terrainHeight, newHeight),
                                       entity.position.z);
    }

    public bool Reachable(DVector3 position) {
        return true;
    }

    public bool MoveTo(DVector3 position) {
        target = World.current.map.WrapPosition(position);
        return true;
    }

    public void Stop() {
        target = entity.position;
    }

    public override uint Checksum() {
        return moveSpeed.Checksum();
    }
}

}
