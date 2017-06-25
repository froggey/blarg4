using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class NamePrefabMap {
    public string name;
    public GameObject prefab;
}

class Testshit: MonoBehaviour, Game.IWorldEventListener {
    public Game.Map map;
    public Game.Navigation navi;
    public Game.World world;

    public NamePrefabMap[] hitscanEffects = null;
    Dictionary<string, GameObject> _hitscanEffects = new Dictionary<string, GameObject>();

    void Awake() {
        try {
            LoadMap();
        } catch(System.Exception e) {
            Debug.Log("Failed to load map. regenerating: " + e);
            RegenerateMap();
        }
        world = new Game.World(map, navi);
        world.eventListener = this;

        foreach(var foo in hitscanEffects) {
            _hitscanEffects.Add(foo.name, foo.prefab);
        }
    }

    void LoadMap() {
        using (var reader = new System.IO.BinaryReader(System.IO.File.OpenRead("test.map"))) {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            map = new Game.Map(width, height);
            map.ReadData(reader);
            navi = new Game.Navigation(map);
            navi.ReadData(reader);
        }
    }

    void RegenerateMap() {
        var dim = 1024;
        map = new Game.Map(dim, dim);
        for(int i = 0; i < 32; i += 1) {
            for(int j = 0; j < dim; j += 1) {
                // Walls. Sloped'+' shape in the middle.
                map.SetRawHeight(i+dim/2-32,j,i*2);
                map.SetRawHeight(j,i+dim/2-32,i*2);
                map.SetRawHeight(32-i+dim/2,j,i*2);
                map.SetRawHeight(j,32-i+dim/2,i*2);
            }
        }
        for(int j = 0; j < dim; j += 1) {
            // Wall midline.
            map.SetRawHeight(dim/2,j,64);
            map.SetRawHeight(j,dim/2,64);
        }
        for(int i = 0; i < 64; i += 1) {
            for(int j = 0; j < 64; j += 1) {
                var dist = Mathf.Sqrt(j*j + i*i);
                if(dist < 64) {
                    // Circular pool at the corners.
                    map.SetRawHeight(i,j, -(int)(64 - dist) / 2);
                    map.SetRawHeight(i,-j, -(int)(64 - dist) / 2);
                    map.SetRawHeight(-i,j, -(int)(64 - dist) / 2);
                    map.SetRawHeight(-i,-j, -(int)(64 - dist) / 2);
                }
            }
        }

        navi = new Game.Navigation(map);
        navi.ComputeReachability();

        using (var writer = new System.IO.BinaryWriter(System.IO.File.Create("test.map"))) {
            writer.Write(map.width);
            writer.Write(map.depth);
            map.WriteData(writer);
            navi.WriteData(writer);
        }
    }

    void SerializeProto(Game.EntityPrototype proto) {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(proto, Newtonsoft.Json.Formatting.Indented);
        using (var writer = new System.IO.StreamWriter(System.IO.Path.Combine("Units", proto.name + ".json"))) {
            writer.Write(json);
        }
    }

    void LoadUnits() {
        foreach(var unit_file in System.IO.Directory.GetFiles("Units", "*.json")) {
            Debug.Log("Loading " + unit_file + "...");
            var json = System.IO.File.ReadAllText(unit_file);
            var proto = Newtonsoft.Json.JsonConvert.DeserializeObject<Game.EntityPrototype>(json);
            world.entityPrototypes.Add(proto.name, proto);
        }
    }

    PlayerInterface piface;
    KeyboardMove theCamera;

    void Start() {
        piface = Object.FindObjectOfType<PlayerInterface>();
        theCamera = Object.FindObjectOfType<KeyboardMove>();
        var mr = (UnityInterwork.MapRenderer)Object.FindObjectOfType<UnityInterwork.MapRenderer>();
        mr.map = map;
        var rp = (UnityInterwork.ReachabilityProjector)Object.FindObjectOfType<UnityInterwork.ReachabilityProjector>();
        if(rp != null) {
            rp.map = map;
            rp.navigation = navi;
        }

        minimapTerrainTexture = new Texture2D(map.width, map.depth);
        for(var x = 0; x < map.width; x += 1) {
            for(var z = 0; z < map.depth; z += 1) {
                var height = (float)map.Height(new Game.DVector3(x,0,z));
                if(height < 0) {
                    minimapTerrainTexture.SetPixel(x,z, Color.blue);
                } else {
                    var shade = Mathf.Clamp01(height / (float)Game.Map.maxHeight);
                    minimapTerrainTexture.SetPixel(x,z, new Color(shade, 0.5f + shade / 2, shade));
                }
            }
        }
        minimapTerrainTexture.Apply();
        minimapTerrain.texture = minimapTerrainTexture;

        world.resourceNameToId.Add("Metal", 1);
        world.resourceNameToId.Add("Smoke", 2);

        LoadUnits();

        SpawnTeam(1);
        SpawnTeam(2);
        SpawnTeam(3);
        SpawnTeam(4);

        world.Tick();

        SpawnMetalSource(192, 256);
        SpawnMetalSource(832, 256);
        SpawnMetalSource(192, 768);
        SpawnMetalSource(832, 768);

        SpawnSmokeSource(256, 192);
        SpawnSmokeSource(768, 192);
        SpawnSmokeSource(768, 832);
        SpawnSmokeSource(256, 832);

        SpawnWizard(256, 256, 1);
        SpawnTruck(768, 256, 2);
        SpawnTruck(256, 768, 3);
        SpawnWizard(768, 768, 4);
    }

    void SpawnTeam(int team) {
        var ent = new Game.Entity(team, new Game.DVector3(0, 0, 0), "Team");
        ent.AddComponent(new Game.Team(ent));
        ent.AddComponent(new Game.ResourcePool(ent, 1, 1000));
        ent.AddComponent(new Game.ResourcePool(ent, 2, 50));
    }

    void SpawnMetalSource(int x, int z) {
        world.Instantiate("MetalSource", 0, new Game.DVector3(x, 0, z));
    }

    void SpawnSmokeSource(int x, int z) {
        world.Instantiate("SmokeSource", 0, new Game.DVector3(x, 0, z));
    }

    void SpawnWizard(int x, int z, int team) {
        world.Instantiate("WizardTower", team, new Game.DVector3(x, 0, z));
        world.Instantiate("Wizard", team, new Game.DVector3(x, 0, z + 32));
    }

    void SpawnTruck(int x, int z, int team) {
        world.Instantiate("ConstructionYard", team, new Game.DVector3(x, 0, z));
    }

    public void BeginPlacement(string prototype, System.Action<Vector3> action_cb, System.Func<Vector3, bool> valid_cb, System.Action cancel_cb) {
        if(piface.placement_go != null) {
            piface.placement_cancel_cb();
            Destroy(piface.placement_go);
        }
        var placement_go = Instantiate(Resources.Load<GameObject>(prototype));
        piface.placement_go = placement_go;
        piface.placement_action_cb = action_cb;
        piface.placement_valid_cb = valid_cb;
        piface.placement_cancel_cb = cancel_cb;
    }

    Dictionary<Game.Entity, UnityInterwork.EntityMirror> entityToUnity = new Dictionary<Game.Entity, UnityInterwork.EntityMirror>();
    Dictionary<Game.Entity, UnityInterwork.MinimapDot> entityToMinimap = new Dictionary<Game.Entity, UnityInterwork.MinimapDot>();

    public GameObject minimapDot = null;
    public RectTransform minimapTransform = null;
    public UnityEngine.UI.RawImage minimapTerrain = null;
    public Texture2D minimapTerrainTexture = null;
    public RectTransform minimapContainerTransform = null;

    UnityInterwork.MinimapController minimapController = new UnityInterwork.MinimapController();

    public void EntityCreated(Game.Entity e) {
        Debug.Log("Created " + e.modelName + " prefab is "+ Resources.Load<GameObject>(e.modelName));
        var go = Instantiate(Resources.Load<GameObject>(e.modelName));
        var mirror = go.GetComponent<UnityInterwork.EntityMirror>();
        mirror.entity = e;
        entityToUnity.Add(e, mirror);

        var minimap_go = Instantiate(minimapDot);
        minimap_go.transform.SetParent(minimapContainerTransform, false);
        var minimap_dot = minimap_go.GetComponent<UnityInterwork.MinimapDot>();
        minimap_dot.entity = e;
        minimap_dot.controller = minimapController;
        entityToMinimap.Add(e, minimap_dot);
    }

    public void EntityDestroyed(Game.Entity e) {
        Debug.Log("Destroying " + e.modelName);
        var mirror = entityToUnity[e];
        entityToUnity.Remove(e);
        mirror.Destroyed();
        Destroy(entityToMinimap[e].gameObject);
        entityToMinimap.Remove(e);
    }

    void HitscanEffect(Game.HitscanWeapon weapon, string effectName, Game.Entity target, float duration) {
        GameObject effectPrefab = null;
        if(!_hitscanEffects.TryGetValue(effectName, out effectPrefab)) {
            Debug.Log("Missing hitscan effect " + effectName);
            return;
        }
        var go = Instantiate(effectPrefab, (Vector3)weapon.entity.position, Quaternion.identity);
        var effect = go.GetComponent<HitscanEffect>();
        effect.weapon = weapon;
        effect.target = target;
        effect.duration = duration;
    }

    public void HitscanBurstFire(Game.HitscanWeapon weapon, string effect, Game.Entity target) {
        Debug.Log("Hitscan burst fire "+ effect);
        HitscanEffect(weapon, effect, target, -1);
    }

    public void HitscanSustainedFire(Game.HitscanWeapon weapon, string effect, Game.Entity target, Game.DReal duration) {
        Debug.Log("Hitscan sustain fire "+ effect + " " + duration);
        HitscanEffect(weapon, effect, target, (float)duration);
    }

    public void Animate(Game.Entity e, string animation) {
        entityToUnity[e].gameObject.SendMessage("Animation" + animation);
    }

    public void StopCommand(UnityInterwork.EntityMirror em) {
        em.entity.StopCommand();
    }

    public void MoveCommand(UnityInterwork.EntityMirror em, Vector3 point) {
        em.entity.MoveCommand((Game.DVector3)point);
    }

    public void AttackCommand(UnityInterwork.EntityMirror em, UnityInterwork.EntityMirror target) {
        em.entity.AttackCommand(target.entity);
    }

    public void DeployCommand(UnityInterwork.EntityMirror em, Vector3 point) {
        em.entity.DeployCommand((Game.DVector3)point);
    }

    public void BuildCommand(UnityInterwork.EntityMirror em, int id, Vector3 point) {
        em.entity.BuildCommand(id, (Game.DVector3)point);
    }

    float timeSlop = 0.0f;

    public UnityEngine.UI.Text resourceBar = null;
    public UnityEngine.UI.Text statusBar = null;

    float GetResource(int team, string resource) {
        var rid = world.resourceNameToId[resource];
        var team_obj = world.entities.First(e => e.team == team && e.GetComponent<Game.Team>() != null);
        var pool = team_obj.GetComponents<Game.ResourcePool>().First(p => p.resourceId == rid);
        return (float)pool.fill;
    }

    void Update() {
        minimapController.xoff = theCamera.transform.position.x;
        minimapController.zoff = theCamera.transform.position.z;
        minimapTerrain.uvRect = new Rect(-minimapController.xoff / map.width,
                                         -minimapController.zoff / map.depth,
                                         1,1);
        minimapTransform.eulerAngles = new Vector3(0,0,theCamera.currentRotation);

        timeSlop += Time.deltaTime;
        while(timeSlop > (float)Game.World.deltaTime) {
            timeSlop -= (float)Game.World.deltaTime;
            world.Tick();
        }

        resourceBar.text = string.Format("Metal: {0}  Smoke: {1}",
                                         (int)GetResource(2, "Metal"),
                                         (int)GetResource(2, "Smoke"));
    }

    Dictionary<int, Color> teamColours = new Dictionary<int, Color>();

    public Color TeamColour(int team) {
        if(team <= 0) {
            return Color.white;
        }
        if(!teamColours.ContainsKey(team)) {
            teamColours.Add(team, Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f));
        }
        return teamColours[team];
    }

    public bool enableInterpolation = true;
}
