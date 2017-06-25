using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HealthBar: MonoBehaviour {
    public Image redPart;
    public Image greenPart;

    public float value;

    void Update() {
        redPart.fillAmount = 1 - value;
        greenPart.fillAmount = value;
    }
}
