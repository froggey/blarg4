using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public enum BuildMode {
    // Place randomly around the producer, after the build time passes.
    SPAWN,
    // Immediate place randomly around the producer,
    // make producer unavailable for the build time.
    SPAWN_IMMEDIATE,
    // Construct, then place at the specified location.
    // Red Alert 1/2 style building.
    BUILD_THEN_PLACE,
    // Construct on the map at the specified location.
    // Age of Empires style building (but not quite, still tied to the factory).
    BUILD_IN_PLACE,
    // Immediate place at the specified location,
    // make producer unavailable for the build time.
    BUILD_IMMEDIATE,
}

public class Entity {
    public readonly int eid;
    public readonly string modelName;
    public readonly EntityPrototype prototype;
    public int team;

    public bool isAlive { get; private set; }

    DVector3 _position;
    public DVector3 position {
        get { return _position; }
        set {
            _position = World.current.map.WrapPosition(value);
        }
    }

    // 0 degrees is north/+Z.
    public DReal _rotation;
    public DReal rotation {
        get { return _rotation; }
        set { _rotation = DReal.Repeat(value, 360); }
    }

    public DVector3 faceDirection {
        get {
            var twoD = DVector2.FromAngle(DReal.Radians((360 - rotation) + 90));
            return new DVector3(twoD.x, 0, twoD.y);
        }
        set { rotation = 360 - (DReal.Degrees(DVector2.ToAngle(new DVector2(value.x, value.z))) - 90); }
    }

    List<Component> _components = new List<Component>();

    public IEnumerable<Component> components {
        get { return _components; }
    }

    public Entity(int team, DVector3 position, string modelName) {
        this.eid = World.current.RegisterEntity(this);
        this.team = team;
        this.position = position;
        this.modelName = modelName;
        this.isAlive = true;
        this.prototype = null;
    }

    public Entity(int team, DVector3 position, EntityPrototype prototype) {
        this.eid = World.current.RegisterEntity(this);
        this.team = team;
        this.position = position;
        this.isAlive = true;
        this.prototype = prototype;
        this.modelName = prototype.name;
    }

    public DVector3 RandomSpawnPosition(DReal min, DReal max) {
        for(var attempt = 0; attempt < 100; attempt += 1) {
            var angle = World.current.RandomValue() * 360;
            var radius = World.current.RandomRange(min, max);
            var x = this.position.x + DReal.Cos(angle) * radius;
            var z = this.position.z + DReal.Sin(angle) * radius;
            var height = World.current.map.Height(new DVector3(x, 0, z));
            var p = new DVector3(x, height, z);
            if(World.current.navigation.Reachability(this.position) == World.current.navigation.Reachability(p)) {
                return p;
            }
        }
        UnityEngine.Debug.Log("Failed to generate random position.");
        return this.position;
    }

    // Range to target, adjusted by collider size.
    public DReal Range(Entity target) {
        var dist = World.current.map.Distance(position, target.position);
        var local_collider = GetComponent<Collider>();
        if(local_collider != null) {
            dist -= local_collider.radius;
        }
        var target_collider = target.GetComponent<Collider>();
        if(target_collider != null) {
            dist -= target_collider.radius;
        }
        return DReal.Max(0, dist);
    }

    public T AddComponent<T>(T component) where T: Component {
        if(component == null) {
            throw new Exception("null component?");
        }
        _components.Add(component);
        return component;
    }

    public T GetComponent<T>() where T: class {
        foreach(var comp in components) {
            if((Object)comp is T) {
                return (T)(Object)comp;
            }
        }
        return null;
    }

    public IEnumerable<T> GetComponents<T>() where T: class {
        foreach(var comp in components) {
            if((Object)comp is T) {
                yield return (T)(Object)comp;
            }
        }
    }

    public void Destroy() {
        if(isAlive) {
            World.current.RemoveEntity(this);
            isAlive = false;
        }
    }

    public void Damage(DReal amount, Entity source = null) {
        var health = GetComponent<Health>();
        if(health != null) {
            health.current -= amount;
            if(source != null) {
                if(!health.damageDealers.ContainsKey(source)) {
                    health.damageDealers.Add(source, 0);
                }
                health.damageDealers[source] += amount;
            }
        }
    }

    public void OnCreate() {
        foreach(var comp in components) {
            comp.OnCreate();
        }
    }

    public void OnTick() {
        foreach(var comp in components) {
            comp.OnTick();
        }
    }

    public void OnDestroy() {
        foreach(var comp in components) {
            comp.OnDestroy();
        }
    }

    public void OnCollision(Entity other) {
        foreach(var comp in components) {
            comp.OnCollision(other);
        }
    }

    public void StopCommand() {
        foreach(var c in components) {
            c.StopCommand();
        }
    }

    public void MoveCommand(DVector3 position) {
        position = World.current.map.WrapPosition(position);
        foreach(var c in components) {
            c.MoveCommand(position);
        }
    }

    public void AttackCommand(Entity target) {
        foreach(var c in components) {
            c.AttackCommand(target);
        }
    }

    public void DeployCommand(DVector3 position) {
        position = World.current.map.WrapPosition(position);
        foreach(var c in components) {
            c.DeployCommand(position);
        }
    }

    public void BuildCommand(int id, DVector3 position) {
        position = World.current.map.WrapPosition(position);
        foreach(var c in components) {
            c.BuildCommand(id, position);
        }
    }

    public uint Checksum() {
        uint checksum = (uint)eid ^ position.Checksum();
        foreach(var comp in components) {
            checksum ^= comp.Checksum();
        }
        return checksum;
    }
}

public class ResourceSet {
    public KeyValuePair<int,DReal>[] resources;

    public DReal this[int resource] {
        get { return resources.Single(kv => kv.Key == resource).Value; }
    }

    public DReal total {
        get { return resources.Select(kv => kv.Value).Aggregate((DReal)0, (prod, next) => prod + next); }
    }
}

[Serializable]
public class EntityPrototype {
    public string name = "";
    public Dictionary<string, string> resources = new Dictionary<string, string>();
    public Dictionary<string, string> data = new Dictionary<string, string>();
    public List<ComponentPrototype> components = new List<ComponentPrototype>();

    public BuildMode BuildMode() {
        var mode = data["buildMode"];
        if(mode == "spawn") {
            return Game.BuildMode.SPAWN;
        } else if(mode == "spawnImmediate") {
            return Game.BuildMode.SPAWN_IMMEDIATE;
        } else if(mode == "buildThenPlace") {
            return Game.BuildMode.BUILD_THEN_PLACE;
        } else if(mode == "buildInPlace") {
            return Game.BuildMode.BUILD_IN_PLACE;
        } else if(mode == "buildImmediate") {
            return Game.BuildMode.BUILD_IMMEDIATE;
        } else {
            throw new Exception("Unknown buld mode " + mode);
        }
    }
}

[Serializable]
public class ComponentPrototype {
    public string kind = "";
    public Dictionary<string, string> data = new Dictionary<string, string>();
}

}