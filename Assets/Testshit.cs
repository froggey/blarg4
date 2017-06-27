using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class Testshit: MonoBehaviour, Game.IWorldEventListener {
    public Game.Map map;
    public Game.Navigation navi;
    public Game.World world;

    public GameObject minimapPrefab;

    public RectTransform minimapContainer;

    WorldManager worldManager;
    MinimapManager minimapManager;

    ComSat comsat;

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
        var mm_go = Instantiate(minimapPrefab);
        mm_go.transform.SetParent(minimapContainer, false);
        minimapManager = mm_go.GetComponent<MinimapManager>();
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

    void LoadResources() {
        Debug.Log("Loading resources...");
        var resources_json = System.IO.File.ReadAllText("Resources.json");
        var resources = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(resources_json);

        for(var i = 0; i < resources.Count; i += 1) {
            Debug.Log(resources[i] + " = " + i);
            world.resourceNameToId.Add(resources[i], i);
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

    int[] teamX = { 256, 768, 256, 768 };
    int[] teamY = { 256, 256, 768, 768 };

    bool HaveTeam(int team) {
        if(ComSat.instance == null) {
            return true;
        } else {
            return ComSat.instance.players.Any(p => p.team == team);
        }
    }

    void Start() {
        piface = GetComponent<PlayerInterface>();

        comsat = Object.FindObjectOfType<ComSat>();

        var mr = Object.FindObjectOfType<UnityInterwork.MapRenderer>();
        if(mr != null) {
            mr.map = map;
        }
        var rp = Object.FindObjectOfType<UnityInterwork.ReachabilityProjector>();
        if(rp != null) {
            rp.map = map;
            rp.navigation = navi;
        }

        LoadResources();
        LoadUnits();

        for(var team = 1; team <= 4; team += 1) {
            if(HaveTeam(team)) {
                SpawnTeam(team);
            }
        }

        world.Tick();

        SpawnMetalSource(192, 256);
        SpawnMetalSource(832, 256);
        SpawnMetalSource(192, 768);
        SpawnMetalSource(832, 768);

        SpawnSmokeSource(256, 192);
        SpawnSmokeSource(768, 192);
        SpawnSmokeSource(768, 832);
        SpawnSmokeSource(256, 832);

        if(ComSat.instance == null) {
            for(var team = 1; team <= 4; team += 1) {
                if((team & 1) == 0) {
                    SpawnTruck(teamX[team-1], teamY[team-1], team);
                } else {
                    SpawnWizard(teamX[team-1], teamY[team-1], team);
                }
            }
        } else {
            var spawned_teams = new List<int>();

            foreach(var player in ComSat.instance.players.OrderBy(p => p.id)) {
                if(player.team == -1) {
                    continue;
                }
                if(spawned_teams.Contains(player.team)) {
                    continue;
                }
                if(player.faction == 1) {
                    SpawnTruck(teamX[player.team-1], teamY[player.team-1], player.team);
                } else {
                    SpawnWizard(teamX[player.team-1], teamY[player.team-1], player.team);
                }
            }
        }
    }

    void SpawnTeam(int team) {
        world.Instantiate("Team", team, new Game.DVector3(0, 0, 0));
    }

    void SpawnMetalSource(int x, int z) {
        world.Instantiate("MetalSource", 0, new Game.DVector3(x, 0, z));
    }

    void SpawnSmokeSource(int x, int z) {
        world.Instantiate("SmokeSource", 0, new Game.DVector3(x, 0, z));
    }

    void SpawnWizard(int x, int z, int team) {
        world.Instantiate("WizardTower", team, new Game.DVector3(x, 0, z));
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
        if(comsat == null) {
            em.entity.StopCommand();
        } else {
            comsat.SendStopCommand(em.entity.eid);
        }
    }

    public void MoveCommand(UnityInterwork.EntityMirror em, Vector3 point) {
        if(comsat == null) {
            em.entity.MoveCommand((Game.DVector3)point);
        } else {
            comsat.SendMoveCommand(em.entity.eid, (Game.DVector3)point);
        }
    }

    public void AttackCommand(UnityInterwork.EntityMirror em, UnityInterwork.EntityMirror target) {
        if(comsat == null) {
            em.entity.AttackCommand(target.entity);
        } else {
            comsat.SendAttackCommand(em.entity.eid, target.entity.eid);
        }
    }

    public void DeployCommand(UnityInterwork.EntityMirror em, Vector3 point) {
        if(comsat == null) {
            em.entity.DeployCommand((Game.DVector3)point);
        } else {
            comsat.SendDeployCommand(em.entity.eid, (Game.DVector3)point);
        }
    }

    public void BuildCommand(UnityInterwork.EntityMirror em, int id, Vector3 point) {
        if(comsat == null) {
            em.entity.BuildCommand(id, (Game.DVector3)point);
        } else {
            comsat.SendBuildCommand(em.entity.eid, id, (Game.DVector3)point);
        }
    }

    public UnityEngine.UI.Text resourceBar = null;
    public UnityEngine.UI.Text statusBar = null;

    float GetResource(int team, string resource) {
        var rid = world.resourceNameToId[resource];
        var team_obj = world.entities.First(e => e.team == team && e.GetComponent<Game.Team>() != null);
        var pool = team_obj.GetComponents<Game.ResourcePool>().First(p => p.resourceId == rid);
        return (float)pool.fill;
    }

    float timeSlop = 0;

    void Update() {
        if(comsat == null) {
            timeSlop += Time.deltaTime;
            while(timeSlop > (float)Game.World.deltaTime) {
                timeSlop -= (float)Game.World.deltaTime;
                Game.World.current.Tick();
            }
        }

        var team = 1;
        if(ComSat.instance != null) {
            team = ComSat.instance.localPlayer.team;
        }

        if(team != -1) {
            resourceBar.text = string.Format("Metal: {0}  Smoke: {1}",
                                             (int)GetResource(team, "Metal"),
                                             (int)GetResource(team, "Smoke"));
        }
    }

    Dictionary<int, Color> teamColours = new Dictionary<int, Color>();

    public Color TeamColour(int team) {
        switch(team) {
        case 0: return Color.white;
        case 1: return Color.green;
        case 2: return Color.red;
        case 3: return Color.blue;
        case 4: return Color.yellow;
        }
        if(!teamColours.ContainsKey(team)) {
            teamColours.Add(team, Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f));
        }
        return teamColours[team];
    }

    public bool enableInterpolation = true;
}
