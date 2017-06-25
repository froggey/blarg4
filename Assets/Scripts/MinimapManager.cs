using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class MinimapManager: MonoBehaviour, Game.IWorldEventListener {
    public Game.Map map;

    public GameObject minimapDot = null;
    public RectTransform minimapTransform = null;
    public UnityEngine.UI.RawImage minimapTerrain = null;
    public Texture2D minimapTerrainTexture = null;
    public RectTransform minimapContainerTransform = null;

    Dictionary<Game.Entity, UnityInterwork.MinimapDot> entityToMinimap = new Dictionary<Game.Entity, UnityInterwork.MinimapDot>();
    UnityInterwork.MinimapController minimapController = new UnityInterwork.MinimapController();

    KeyboardMove theCamera;

    void Start() {
        theCamera = Object.FindObjectOfType<KeyboardMove>();

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
    }

    public void EntityCreated(Game.Entity e) {
        var minimap_go = Instantiate(minimapDot);
        minimap_go.transform.SetParent(minimapContainerTransform, false);
        var minimap_dot = minimap_go.GetComponent<UnityInterwork.MinimapDot>();
        minimap_dot.entity = e;
        minimap_dot.controller = minimapController;
        entityToMinimap.Add(e, minimap_dot);
    }

    public void EntityDestroyed(Game.Entity e) {
        Destroy(entityToMinimap[e].gameObject);
        entityToMinimap.Remove(e);
    }

    public void HitscanBurstFire(Game.HitscanWeapon weapon, string effect, Game.Entity target) {
    }

    public void HitscanSustainedFire(Game.HitscanWeapon weapon, string effect, Game.Entity target, Game.DReal duration) {
    }

    public void Animate(Game.Entity e, string animation) {
    }

    void Update() {
        minimapController.xoff = theCamera.transform.position.x;
        minimapController.zoff = theCamera.transform.position.z;
        minimapTerrain.uvRect = new Rect(-minimapController.xoff / map.width,
                                         -minimapController.zoff / map.depth,
                                         1,1);
        minimapTransform.eulerAngles = new Vector3(0,0,theCamera.currentRotation);
    }
}
