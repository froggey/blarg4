using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

public class ReachabilityProjector: MonoBehaviour {
    public Texture2D tex;

    public Game.Map map = null;
    public Game.Navigation navigation = null;

    Dictionary<int, Color> colour_map = new Dictionary<int, Color>();

    Color ColorForReachability(int tag) {
        if(tag == Game.Navigation.impassibleTag) {
            return Color.black;
        } else if(tag == 0) {
            return Color.magenta;
        } else {
            if(!colour_map.ContainsKey(tag)) {
                colour_map.Add(tag, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            }
            return colour_map[tag];
        }
    }

    bool did_build = false;
    void Update() {
        if(navigation != null && map != null && !did_build) {
            did_build = true;
            tex = new Texture2D(map.width / Game.Navigation.granularity, map.depth / Game.Navigation.granularity);
            for(var x = 0; x < map.width / Game.Navigation.granularity; x += 1) {
                for(var z = 0; z < map.depth / Game.Navigation.granularity; z += 1) {
                    tex.SetPixel(x,z, ColorForReachability(navigation.Reachability(new Game.DVector3(x * Game.Navigation.granularity,
                                                                                                     0,
                                                                                                     z * Game.Navigation.granularity))));
                }
            }
            tex.Apply();
        }
    }
}

}
