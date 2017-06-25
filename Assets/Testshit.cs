using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class Testshit: MonoBehaviour, Game.IWorldEventListener {
    public Game.Map map;
    public Game.Navigation navi;
    public Game.World world;

    WorldManager worldManager;
    MinimapManager minimapManager;

    void Awake() {
        try {
            LoadMap();
        } catch(System.Exception e) {
            Debug.Log("Failed to load map. regenerating: " + e);
            RegenerateMap();
        }
        world = new Game.World(map, navi);
        world.eventListener = this;

        worldManager = GetComponent<WorldManager>();
        minimapManager = GetComponent<MinimapManager>();
        minimapManager.map = map;
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

    void Start() {
        piface = Object.FindObjectOfType<PlayerInterface>();
        var mr = (UnityInterwork.MapRenderer)Object.FindObjectOfType<UnityInterwork.MapRenderer>();
        if(mr != null) {
            mr.map = map;
        }
        var rp = (UnityInterwork.ReachabilityProjector)Object.FindObjectOfType<UnityInterwork.ReachabilityProjector>();
        if(rp != null) {
            rp.map = map;
            rp.navigation = navi;
        }

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
        var placement_go = Instantiate(Resources.Load<GameObject>("Models/" + prototype));
        piface.placement_go = placement_go;
        piface.placement_action_cb = action_cb;
        piface.placement_valid_cb = valid_cb;
        piface.placement_cancel_cb = cancel_cb;
    }


    public void EntityCreated(Game.Entity e) {
        worldManager.EntityCreated(e);
        minimapManager.EntityCreated(e);
    }

    public void EntityDestroyed(Game.Entity e) {
        Debug.Log("Destroying " + e.modelName);
        worldManager.EntityDestroyed(e);
        minimapManager.EntityDestroyed(e);
    }

    public void HitscanBurstFire(Game.HitscanWeapon weapon, string effect, Game.Entity target) {
        worldManager.HitscanBurstFire(weapon, effect, target);
        minimapManager.HitscanBurstFire(weapon, effect, target);
    }

    public void HitscanSustainedFire(Game.HitscanWeapon weapon, string effect, Game.Entity target, Game.DReal duration) {
        worldManager.HitscanSustainedFire(weapon, effect, target, duration);
        minimapManager.HitscanSustainedFire(weapon, effect, target, duration);
    }

    public void Animate(Game.Entity e, string animation) {
        worldManager.Animate(e, animation);
        minimapManager.Animate(e, animation);
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
