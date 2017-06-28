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
        for(var x = -1; x <= 1; x += 1) {
            for(var z = -1; z <= 1; z += 1) {
                SpawnTerrain(x * 1024, -128, z * 1024);
            }
        }
    }
}
