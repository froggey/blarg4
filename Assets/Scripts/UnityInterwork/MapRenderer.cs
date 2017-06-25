using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

[RequireComponent(typeof(Terrain))]
public class MapRenderer: MonoBehaviour {
    int mapVersion;

    Game.Map _map = null;
    public Game.Map map {
        get { return _map; }
        set {
            _map = value;
            mapVersion = -1;
        }
    }

    TerrainData td;

    void Start() {
        td = GetComponent<Terrain>().terrainData;
    }

    void Update() {
        if(map != null && mapVersion != map.version) {
            RebuildTerrain();
            mapVersion = map.version;
        }
    }

    void RebuildTerrain() {
        var dim = (int)Mathf.Max((int)map.width, (int)map.depth);
        td.heightmapResolution = dim + 1;
        td.size = new Vector3(dim, 256, dim);
        var heights = new float[dim+1,dim+1];
        for(var z = 0; z < dim+1; z += 1) {
            for(var x = 0; x < dim+1; x += 1) {
                heights[z,x] = ((float)map.Height(new Game.DVector3(x,0,z)) + 128) / 256;
            }
        }
        td.SetHeights(0,0, heights);
    }
}

}
