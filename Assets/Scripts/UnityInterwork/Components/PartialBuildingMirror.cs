using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class PartialBuildingMirror: MonoBehaviour {
    public Game.PartialBuilding component;

    // This is set automatically by EntityMirror...
    public GameObject resourceBarPrefab;
    public RectTransform canvasTransform;

    public ResourceBar resourceBar;

    void Start() {
        transform.localScale = new Vector3(1,0,1);
        var go = Instantiate(resourceBarPrefab);
        go.transform.SetParent(canvasTransform, false);
        resourceBar = go.GetComponent<ResourceBar>();
        resourceBar.value = 0.0f;
        UpdateBarPosition();
    }

    void OnDestroy() {
        if(resourceBar != null) {
            Destroy(resourceBar.gameObject);
        }
    }

    // FIXME
    void UpdateBarPosition() {
        var viewport_position = Camera.main.WorldToViewportPoint(transform.position);
        var screen_position = new Vector2(
            ((viewport_position.x * canvasTransform.sizeDelta.x) - (canvasTransform.sizeDelta.x * 0.5f)),
            ((viewport_position.y * canvasTransform.sizeDelta.y) - (canvasTransform.sizeDelta.y * 0.5f)));

        ((RectTransform)resourceBar.transform).anchoredPosition = screen_position;
    }

    void Update() {
        transform.localScale = new Vector3(1,(float)component.buildProgress,1);
        resourceBar.value = (float)component.buildProgress;
        UpdateBarPosition();
    }
}

}
