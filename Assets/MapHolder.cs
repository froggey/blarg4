using UnityEngine;
using System.Collections.Generic;

public class MapHolder : MonoBehaviour {
    public Game.Map map;

    void Awake() {
        map = new Game.Map(1024, 1024);
        for(int i = 0; i < 64; i += 1) {
            for(int j = 0; j < 1024; j += 1) {
                map.SetRawHeight(i+512,j,32);
                map.SetRawHeight(j,i+512,32);
            }
        }
    }
}
