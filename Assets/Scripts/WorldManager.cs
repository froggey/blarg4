using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Manages units & effects visible in the world.
class WorldManager: MonoBehaviour, Game.IWorldEventListener {
    // 4 quadrants.
    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity1 = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();
    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity2 = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();
    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity3 = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();
    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity4 = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();

    public void EntityCreated(Game.Entity e) {
        var prefab = Resources.Load<GameObject>("Models/" + e.modelName);
        Debug.Log("Created " + e.modelName + " prefab is "+ prefab);
        var go1 = Instantiate(prefab);
        var mirror1 = go1.GetComponent<UnityInterwork.EntityMirror>();
        mirror1.entity = e;
        mirror1.positionAdjust = new Vector3(0,0,0);
        entityToUnity1.Add(e, mirror1);
        var go2 = Instantiate(prefab);
        var mirror2 = go2.GetComponent<UnityInterwork.EntityMirror>();
        mirror2.entity = e;
        mirror2.positionAdjust = new Vector3(-1024,0,0);
        entityToUnity2.Add(e, mirror2);
        var go3 = Instantiate(prefab);
        var mirror3 = go3.GetComponent<UnityInterwork.EntityMirror>();
        mirror3.entity = e;
        mirror3.positionAdjust = new Vector3(0,0,-1024);
        entityToUnity3.Add(e, mirror3);
        var go4 = Instantiate(prefab);
        var mirror4 = go4.GetComponent<UnityInterwork.EntityMirror>();
        mirror4.entity = e;
        mirror4.positionAdjust = new Vector3(-1024,0,-1024);
        entityToUnity4.Add(e, mirror4);
    }

    public void EntityDestroyed(Game.Entity e) {
        Debug.Log("Destroying " + e.modelName);
        var mirror1 = entityToUnity1[e];
        entityToUnity1.Remove(e);
        mirror1.Destroyed();
        var mirror2 = entityToUnity2[e];
        entityToUnity2.Remove(e);
        mirror2.Destroyed();
        var mirror3 = entityToUnity3[e];
        entityToUnity3.Remove(e);
        mirror3.Destroyed();
        var mirror4 = entityToUnity4[e];
        entityToUnity4.Remove(e);
        mirror4.Destroyed();
    }

    void HitscanEffect(Game.HitscanWeapon weapon, string effectName, Game.Entity target, float duration) {
        GameObject effectPrefab = Resources.Load<GameObject>("HitscanEffects/" + effectName);
        var go1 = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
        var effect1 = go1.GetComponent<HitscanEffect>();
        effect1.weapon = weapon;
        effect1.target = target;
        effect1.duration = duration;
        effect1.positionAdjust = new Vector3(0,0,0);
        var go2 = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
        var effect2 = go2.GetComponent<HitscanEffect>();
        effect2.weapon = weapon;
        effect2.target = target;
        effect2.duration = duration;
        effect2.positionAdjust = new Vector3(0,0,-1024);
        var go3 = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
        var effect3 = go3.GetComponent<HitscanEffect>();
        effect3.weapon = weapon;
        effect3.target = target;
        effect3.duration = duration;
        effect3.positionAdjust = new Vector3(-1024,0,0);
        var go4 = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
        var effect4 = go4.GetComponent<HitscanEffect>();
        effect4.weapon = weapon;
        effect4.target = target;
        effect4.duration = duration;
        effect4.positionAdjust = new Vector3(-1024,0,-1024);
    }

    public void HitscanBurstFire(Game.HitscanWeapon weapon, string effect, Game.Entity target) {
        HitscanEffect(weapon, effect, target, -1);
    }

    public void HitscanSustainedFire(Game.HitscanWeapon weapon, string effect, Game.Entity target, Game.DReal duration) {
        HitscanEffect(weapon, effect, target, (float)duration);
    }

    public void Animate(Game.Entity e, string animation) {
        entityToUnity1[e].gameObject.SendMessage("Animation" + animation);
        entityToUnity2[e].gameObject.SendMessage("Animation" + animation);
        entityToUnity3[e].gameObject.SendMessage("Animation" + animation);
        entityToUnity4[e].gameObject.SendMessage("Animation" + animation);
    }
}
