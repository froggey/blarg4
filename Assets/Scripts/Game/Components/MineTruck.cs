using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {

public class MineTruck: Component {
    IMotor motor;

    KeyValuePair<int, string>[] mineTypes;

    public MineTruck(Entity entity, ComponentPrototype proto): base(entity) {
        motor = entity.GetComponent<IMotor>();
        mineTypes = proto.data
            .Select(kv => new KeyValuePair<int,String>(World.current.resourceNameToId.ContainsKey(kv.Key) ? World.current.resourceNameToId[kv.Key] : -1, kv.Value))
            .Where(kv => kv.Key != -1)
            .OrderBy(kv => kv.Key)
            .ToArray();
    }

    ResourceSource source = null;

    public override void OnTick() {
        if(source != null && (source.occupied || source.Depleted())) {
            source = null;
            motor.Stop();
        }
        if(source != null) {
            var dist = World.current.map.Distance(entity.position, source.entity.position);
            if(dist < 1) {
                var ent = World.current.Instantiate(mineTypes.First(kv => kv.Key == source.resourceId).Value,
                                                    entity.team,
                                                    source.entity.position);
                ent.rotation = entity.rotation;
                var mine = ent.GetComponent<Mine>();
                mine.source = source;
                entity.Destroy();
            } else {
                motor.MoveTo(source.entity.position);
            }
        }
    }

    bool IsResourceCompatible(int id) {
        return mineTypes.Any(kv => kv.Key == id);
    }

    public override void MoveCommand(DVector3 position) {
        source = null;
        motor.MoveTo(position);
    }

    public override void AttackCommand(Entity target) {
        var src = target.GetComponents<ResourceSource>().Where(s => IsResourceCompatible(s.resourceId) && !s.Depleted() && !s.occupied).FirstOrDefault();
        if(src != null) {
            motor.Stop();
            source = src;
        }
    }

    public override uint Checksum() {
        return (uint)0;
    }
}

}
