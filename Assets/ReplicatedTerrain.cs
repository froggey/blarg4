using UnityEngine;

class ReplicatedTerrain: MonoBehaviour {
    public TerrainData terrain;

    void SpawnTerrain(int x, int y, int z) {
        var go = new GameObject();
        go.layer = gameObject.layer;
        go.transform.position = new Vector3(x, y, z);
        var t = go.AddComponent<Terrain>();
        t.terrainData = terrain;
        var tc = go.AddComponent<TerrainCollider>();
        tc.terrainData = terrain;
    }

    void Start() {
        SpawnTerrain(-1024, -128, -1024);
        SpawnTerrain(-1024, -128, 0);
        SpawnTerrain(0, -128, -1024);
        SpawnTerrain(0, -128, 0);
    }
}
