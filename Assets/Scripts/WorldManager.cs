using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Manages units & effects visible in the world.
class WorldManager: MonoBehaviour, Game.IWorldEventListener {
    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();

    public void EntityCreated(Game.Entity e) {
        var prefab = Resources.Load<GameObject>("Models/" + e.modelName);
        Debug.Log("Created " + e.modelName + " prefab is "+ prefab);
        var go = Instantiate(prefab);
        var mirror = go.GetComponent<UnityInterwork.EntityMirror>();
        mirror.entity = e;
        mirror.positionAdjust = new Vector3(0, 0, 0);
        entityToUnity.Add(e, mirror);
    }

    public void EntityDestroyed(Game.Entity e) {
        Debug.Log("Destroying " + e.modelName);
        var mirror = entityToUnity[e];
        entityToUnity.Remove(e);
        mirror.Destroyed();
    }

    void HitscanEffect(Game.HitscanWeapon weapon, string effectName, Game.Entity target, float duration) {
        GameObject effectPrefab = Resources.Load<GameObject>("HitscanEffects/" + effectName);
        for(var x = -1; x <= 1; x += 1) {
            for(var z = -1; z <= 1; z += 1) {
                var go = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
                var effect = go.GetComponent<HitscanEffect>();
                effect.weapon = weapon;
                effect.target = target;
                effect.duration = duration;
                effect.positionAdjust = new Vector3(x*1024, 0, z*1024);
            }
        }
    }

    public void HitscanBurstFire(Game.HitscanWeapon weapon, string effect, Game.Entity target) {
        HitscanEffect(weapon, effect, target, -1);
    }

    public void HitscanSustainedFire(Game.HitscanWeapon weapon, string effect, Game.Entity target, Game.DReal duration) {
        HitscanEffect(weapon, effect, target, (float)duration);
    }

    public void Animate(Game.Entity e, string animation) {
        entityToUnity[e].Animate(animation);
    }
}
