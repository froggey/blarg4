using UnityEngine;
using System.Collections.Generic;

namespace MapEditor {
[RequireComponent(typeof(Terrain))]
class MapEditor: MonoBehaviour {
    public Transform waterQuad = null;
    public Transform cursor = null;
    public Transform cursor2 = null;
    public Transform multiselect = null;

    int mapVersion;
    Game.Map map;

    Terrain terrain;
/*
    Terrain[] terrains = null;

    void Start() {
        map = new Game.Map(32, 32, 4);
        terrain = GetComponent<Terrain>();

        RebuildMapGeometry();
    }

    void RebuildMapGeometry() {
        mapVersion = map.version;

        waterQuad.position = new Vector3((float)map.width / 2, -0.5f, (float)map.depth / 2);
        waterQuad.localScale = new Vector3((float)map.width, (float)map.depth, 1);

        transform.position = new Vector3(0, (float)Game.Map.minHeight, 0);

        var heightRange = Game.Map.maxHeight - Game.Map.minHeight + 1;
        terrain.terrainData.heightmapResolution = (int)Mathf.Max((float)map.width, (float)map.depth) + 1;
        terrain.terrainData.size = new Vector3((float)map.width, (float)heightRange, (float)map.depth);

        var heights = new float[(int)map.depth+1, (int)map.width+1];
        for(var z = 0; z < (int)map.depth + 1; z += 1) {
            for(var x = 0; x < (int)map.width + 1; x += 1) {
                heights[z,x] = (float)((map.Height(new Game.DVector3(x,0,z)) - Game.Map.minHeight) / heightRange);
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);

        if(terrains != null) {
            foreach(var t in terrains) {
                GameObject.Destroy(t.gameObject);
            }
            terrains = null;
        }
        terrains = new Terrain[8];
        terrains[0] = CreateTerrain("Terrain North",  terrain.terrainData, (float)map.width, (float)Game.Map.minHeight, 0);
        terrains[1] = CreateTerrain("Terrain East",   terrain.terrainData, 0, (float)Game.Map.minHeight, (float)map.depth);
        terrains[2] = CreateTerrain("Terrain South",  terrain.terrainData, -(float)map.width, (float)Game.Map.minHeight, 0);
        terrains[3] = CreateTerrain("Terrain West",   terrain.terrainData, 0, (float)Game.Map.minHeight, -(float)map.depth);
        terrains[4] = CreateTerrain("Terrain NorthE", terrain.terrainData, (float)map.width, (float)Game.Map.minHeight, (float)map.depth);
        terrains[5] = CreateTerrain("Terrain SouthE", terrain.terrainData, -(float)map.width, (float)Game.Map.minHeight, (float)map.depth);
        terrains[6] = CreateTerrain("Terrain SouthW", terrain.terrainData, -(float)map.width, (float)Game.Map.minHeight, -(float)map.depth);
        terrains[7] = CreateTerrain("Terrain NorthW", terrain.terrainData, (float)map.width, (float)Game.Map.minHeight, -(float)map.depth);
    }

    Terrain CreateTerrain(string name, TerrainData td, float x, float y, float z) {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x,y,z);
        var t = go.AddComponent<Terrain>();
        t.terrainData = td;
        var c = go.AddComponent<TerrainCollider>();
        c.terrainData = td;
        return t;
    }

    void RepositionMultiselect() {
        if(cursor.position == cursor2.position) {
            multiselect.gameObject.SetActive(false);
        } else {
            multiselect.gameObject.SetActive(true);
            multiselect.position = (cursor.position + cursor2.position) / 2 + Vector3.up;
            multiselect.localScale = new Vector3(cursor.position.x - cursor2.position.x,
                                                 cursor.position.z - cursor2.position.z,
                                                 1);
        }
    }

    void Update() {
        if(mapVersion != map.version) {
            RebuildMapGeometry();
        }

        if(Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                Debug.Log("height at " + hit.point + " = " + map.Height(new Game.DVector3((Game.DReal)hit.point.x,(Game.DReal)hit.point.y,(Game.DReal)hit.point.z)));
                var x = ((int)hit.point.x / map.scale) * map.scale;
                var z = ((int)hit.point.z / map.scale) * map.scale;
                var y = (float)map.RawHeight(x,z);
                if(!Input.GetKey(KeyCode.RightShift)) {
                    cursor.position = new Vector3(x, y, z);
                }
                cursor2.position = new Vector3(x, y, z);
            }
            RepositionMultiselect();
        }
        if(Input.GetKeyDown(KeyCode.P)) {
            Raise(8);
        }
        if(Input.GetKeyDown(KeyCode.O)) {
            Raise(-8);
        }
    }

    void Raise(int amount) {
        int x1, x2;
        if(cursor.position.x < cursor2.position.x) {
            x1 = (int)cursor.position.x;
            x2 = (int)cursor2.position.x;
        } else {
            x2 = (int)cursor.position.x;
            x1 = (int)cursor2.position.x;
        }
        int z1, z2;
        if(cursor.position.z < cursor2.position.z) {
            z1 = (int)cursor.position.z;
            z2 = (int)cursor2.position.z;
        } else {
            z2 = (int)cursor.position.z;
            z1 = (int)cursor2.position.z;
        }

        Debug.Log("Raise (" + x1 + "," + z1 + ") - (" + x2 + "," + z2 + ") by " + amount);

        for(var z = z1/map.scale; z <= z2/map.scale; z += 1) {
            for(var x = x1/map.scale; x <= x2/map.scale; x += 1) {
                var p = map.WrapPosition(new Game.DVector3(x*map.scale, 0, z*map.scale));
                var wx = (int)p.x;
                var wz = (int)p.z;
                Debug.Log("Set (" + wx + "," + wz + ") to " + (map.RawHeight(wx,wz) + amount) + " from " + map.RawHeight(wx,wz));
                map.SetRawHeight(wx,wz, map.RawHeight(wx,wz) + amount);
            }
        }

        // Update cursor heights.
        cursor.position = new Vector3(cursor.position.x,
                                      (float)map.Height(new Game.DVector3((Game.DReal)cursor.position.x, (Game.DReal)cursor.position.y, (Game.DReal)cursor.position.z)),
                                      cursor.position.z);
        cursor2.position = new Vector3(cursor2.position.x,
                                       (float)map.Height(new Game.DVector3((Game.DReal)cursor2.position.x, (Game.DReal)cursor2.position.y, (Game.DReal)cursor2.position.z)),
                                       cursor2.position.z);
    }

    */
}
}
