using UnityEngine;
using System.Collections.Generic;

namespace UnityInterwork {

class HealthMirror: InterworkComponent {
    public Game.Health component;

    // This is set automatically by EntityMirror...
    public GameObject healthBarPrefab;
    public RectTransform canvasTransform;

    public HealthBar healthBar;

    public float current;
    public float max;

    void Start() {
        var go = Instantiate(healthBarPrefab);
        go.transform.SetParent(canvasTransform, false);
        healthBar = go.GetComponent<HealthBar>();
        healthBar.value = 1.0f;
        UpdateBarPosition();
    }

    void OnDestroy() {
        if(healthBar != null) {
            Destroy(healthBar.gameObject);
        }
    }

    void UpdateBarPosition() {
        var viewport_position = Camera.main.WorldToViewportPoint(transform.position);
        var screen_position = new Vector2(
            ((viewport_position.x * canvasTransform.sizeDelta.x) - (canvasTransform.sizeDelta.x * 0.5f)),
            ((viewport_position.y * canvasTransform.sizeDelta.y) - (canvasTransform.sizeDelta.y * 0.5f)));

        ((RectTransform)healthBar.transform).anchoredPosition = screen_position;
    }

    void Update() {
        current = (float)component.current;
        max = (float)component.max;

        healthBar.value = current / max;
        UpdateBarPosition();
    }
}

}
