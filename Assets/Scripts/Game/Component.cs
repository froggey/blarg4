using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public abstract class Component {
    public readonly Entity entity;

    public Component(Entity entity) {
        this.entity = entity;
    }

    public virtual void OnCreate() {}
    public virtual void OnTick() {}
    public virtual void OnDestroy() {}
    public virtual void OnCollision(Entity other) {}

    public virtual void StopCommand() {}
    public virtual void MoveCommand(DVector3 position) {}
    public virtual void AttackCommand(Entity target) {}
    public virtual void DeployCommand(DVector3 position) {}
    public virtual void BuildCommand(int id, DVector3 position) {}

    public abstract uint Checksum();
}

}
