using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public interface IWorldEventListener {
    void EntityCreated(Entity e);
    void EntityDestroyed(Entity e);

    void Animate(Entity e, string animation);

    void HitscanBurstFire(HitscanWeapon weapon, string effect, Entity target);
    void HitscanSustainedFire(HitscanWeapon weapon, string effect, Entity target, DReal duration);
}

public class LineCastResult {
    public Entity entity;
    public DVector3 position;
}

public class World {
    public IWorldEventListener eventListener = null;

    int nextEntityId = 1;

    public int currentTick { get; private set; }

    // Seconds per tick.
    public static readonly DReal deltaTime = (DReal)1 / 25;
    public DReal time { get { return currentTick * deltaTime; } }

    public Entity IdToEntity(int eid) {
        Entity result;
        if(eidToEntityMap.TryGetValue(eid, out result)) {
            return result;
        } else {
            return null;
        }
    }

    public static World current { get; private set; }

    public readonly Map map;
    public readonly Navigation navigation;

    public Dictionary<string, EntityPrototype> entityPrototypes = new Dictionary<string, EntityPrototype>();

    public Dictionary<string, int> resourceNameToId = new Dictionary<string, int>();

    public World(Map map, Navigation navigation) {
        this.map = map;
        this.navigation = navigation;
        current = this;
    }

    uint randomValue = 0;

    public DReal RandomValue() {
        randomValue = 22695477 * randomValue + 1;
        return ((DReal)randomValue / 0x10000) % 1; // dreal has a 16 bit fractional part.
    }

    public DReal RandomRange(DReal min, DReal max) {
        return min + RandomValue() * (max - min);
    }

    List<Entity> _entities = new List<Entity>();
    public IEnumerable<Entity> entities {
        get { return _entities; }
    }
    List<Entity> new_entities = new List<Entity>();
    List<Entity> dead_entities = new List<Entity>();
    // Don't iterate over this, ordering is inconsistent.
    Dictionary<int, Entity> eidToEntityMap = new Dictionary<int, Entity>();

    public void Tick() {
        currentTick += 1;
        // Call OnCreate on each new entity.
        while(new_entities.Count != 0) {
            var e = new_entities[new_entities.Count-1];
            new_entities.RemoveAt(new_entities.Count-1);
            e.OnCreate();
            _entities.Add(e);
            eventListener.EntityCreated(e);
        }
        // OnTick.
        foreach(var e in entities) {
            e.OnTick();
        }
        // Perform collision detection.
        for(var i = 0; i < colliders.Count; i += 1) {
            var ci = colliders[i];
            var i_bot = ci.entity.position.y;
            var i_top = i_bot + ci.height;
            for(var j = i+1; j < colliders.Count; j += 1) {
                var cj = colliders[j];
                var j_bot = cj.entity.position.y;
                var j_top = j_bot + cj.height;
                var size = ci.radius + cj.radius;
                var dist_sqr = map.DistanceSqr(ci.entity.position, cj.entity.position);
                if(dist_sqr > size * size) {
                    continue;
                }
                if(DReal.Max(i_bot, j_bot) > DReal.Min(i_top, j_top)) {
                    continue;
                }
                ci.entity.OnCollision(cj.entity);
                cj.entity.OnCollision(ci.entity);
            }
        }
        // Call OnDestroy on each dead entity.
        while(dead_entities.Count != 0) {
            var e = dead_entities[dead_entities.Count-1];
            dead_entities.RemoveAt(dead_entities.Count-1);
            if(new_entities.Contains(e)) {
                new_entities.Remove(e);
            } else {
                e.OnDestroy();
                _entities.Remove(e);
                eidToEntityMap.Remove(e.eid);
                eventListener.EntityDestroyed(e);
            }
        }
    }

    public int RegisterEntity(Entity ent) {
        if(ent == null) {
            throw new Exception("null entity?");
        }
        var eid = nextEntityId;
        nextEntityId += 1;
        eidToEntityMap.Add(eid, ent);
        new_entities.Add(ent);
        return eid;
    }

    public void RemoveEntity(Entity ent) {
        dead_entities.Add(ent);
    }

    public Entity Instantiate(string prototypeName, int team, DVector3 position) {
        var proto = entityPrototypes[prototypeName];
        var ent = new Entity(team, position, proto);
        foreach(var cproto in proto.components) {
            Component comp = null;
            if(cproto.kind == "Wizard") {
                comp = new Wizard(ent, cproto);
            } else if(cproto.kind == "WizardTower") {
                comp = new WizardTower(ent, cproto);
            } else if(cproto.kind == "BasicUnit") {
                comp = new BasicUnit(ent, cproto);
            } else if(cproto.kind == "GroundMotor") {
                comp = new GroundMotor(ent, cproto);
            } else if(cproto.kind == "AirMotor") {
                comp = new AirMotor(ent, cproto);
            } else if(cproto.kind == "ResourceCollectionPoint") {
                comp = new ResourceCollectionPoint(ent, cproto);
            } else if(cproto.kind == "ResourceHarvester") {
                comp = new ResourceHarvester(ent, cproto);
            } else if(cproto.kind == "Truck") {
                comp = new Truck(ent, cproto);
            } else if(cproto.kind == "MineTruck") {
                comp = new MineTruck(ent, cproto);
            } else if(cproto.kind == "Mine") {
                comp = new Mine(ent, cproto);
            } else if(cproto.kind == "Factory") {
                comp = new Factory(ent, cproto);
            } else if(cproto.kind == "ResourceSource") {
                comp = new ResourceSource(ent, cproto);
            } else if(cproto.kind == "ProjectileWeapon") {
                comp = new ProjectileWeapon(ent, cproto);
            } else if(cproto.kind == "Projectile") {
                comp = new Projectile(ent, cproto);
            } else if(cproto.kind == "Collider") {
                comp = new Collider(ent, cproto);
            } else if(cproto.kind == "HitscanWeapon") {
                comp = new HitscanWeapon(ent, cproto);
            } else if(cproto.kind == "AutoTurret") {
                comp = new AutoTurret(ent, cproto);
            } else if(cproto.kind == "Health") {
                comp = new Health(ent, cproto);
            } else if(cproto.kind == "BuildRadius") {
                comp = new BuildRadius(ent, cproto);
            } else if(cproto.kind == "Team") {
                comp = new Team(ent, cproto);
            } else if(cproto.kind == "ResourcePool") {
                comp = new ResourcePool(ent, cproto);
            } else {
                Logger.Log("Unknown component type {0}", cproto.kind);
            }
            ent.AddComponent(comp);
        }
        return ent;
    }

    public ResourceSet ParseResources(Dictionary<string, string> resources) {
        var rs = new ResourceSet();
        rs.resources = resources
            .Select(kv => new KeyValuePair<int,DReal>(World.current.resourceNameToId.ContainsKey(kv.Key) ? World.current.resourceNameToId[kv.Key] : -1, DReal.Parse(kv.Value)))
            .Where(kv => kv.Key != -1)
            .OrderBy(kv => kv.Key)
            .ToArray();
        return rs;
    }

    List<Collider> colliders = new List<Collider>();

    // Cast a line from A to B, checking for collisions with other entities.
    public IEnumerable<LineCastResult> LineCastAll(DVector3 start, DVector3 end, int ignoreTeam = -1) {
        var start2d = new DVector2(start.x, start.z);
        var end2d = new DVector2(end.x, end.z);

        foreach(Collider c in colliders) {
            if(c.entity.team == ignoreTeam) continue;

            var position2d = new DVector2(c.entity.position.x, c.entity.position.z);

            DVector2 result;

            if(Utility.IntersectLineCircle(position2d, c.radius, start2d, end2d, out result)) {
                // Possible hit, work out the height at the intersect.
                // This is probably a little bit off...
                var dist = (start2d - result).magnitude;
                var dir = (end - start).normalized;
                var height = (start + dir * dist).y;
                var bottom = c.entity.position.y;
                var top = bottom + c.height;
                if(bottom <= height && height <= top) {
                    var hit_position = new DVector3(result.x, height, result.y);
                    var r = new LineCastResult();
                    r.entity = c.entity;
                    r.position = hit_position;
                    yield return r;
                }
            }
        }
    }

    public Entity LineCast(DVector3 start, DVector3 end, out DVector3 hitPosition, int ignoreTeam = -1) {
        var result = LineCastAll(start, end, ignoreTeam)
            .OrderBy(r => map.Distance(start, r.position))
            .FirstOrDefault();
        if(result == null) {
            hitPosition = new DVector3(0,0,0);
            return null;
        }
        hitPosition = result.position;
        return result.entity;
    }

    public IEnumerable<Entity> FindEntitiesWithinRadius(DVector3 position, DReal radius, int ignoreTeam = -1) {
        foreach(Collider c in colliders) {
            if(c.entity.team == ignoreTeam) continue;

            var dist = map.Distance(c.entity.position, position);
            if(dist < radius + c.radius) {
                yield return c.entity;
            }
        }
    }

    public void RegisterCollider(Collider collider) {
        if(collider == null) {
            throw new Exception("null collider?");
        }
        colliders.Add(collider);
    }

    public void UnregisterCollider(Collider collider) {
        colliders.Remove(collider);
    }

    public uint Checksum() {
        uint sum = 0;
        sum ^= randomValue;
        sum ^= (uint)currentTick;
        sum ^= map.Checksum();
        sum ^= (uint)_entities.Count;
        foreach(var ent in _entities) {
            sum ^= ent.Checksum();
        }
        return sum;
    }
}

}
