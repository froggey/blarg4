using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityInterwork {

public class ResourceBar: MonoBehaviour {
    public Image barPart;

    public float value;

    void Update() {
        barPart.fillAmount = value;
    }
}

}
