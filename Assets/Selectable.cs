using UnityEngine;

class Selectable: MonoBehaviour {
    public bool selected = false;

    bool selectedState = false;
    Renderer[] renderers = null;

    void Start() {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnSelect() {
        selected = true;
    }

    void OnDeselect() {
        selected = false;
    }

    void Update() {
        if(selectedState != selected) {
            selectedState = selected;
            foreach(var rend in renderers) {
                if(selected) {
                    rend.material.color = Color.green;
                } else {
                    rend.material.color = Color.white;
                }
            }
        }
    }
}
