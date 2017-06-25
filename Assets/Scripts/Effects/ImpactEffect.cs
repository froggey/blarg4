using UnityEngine;

class ImpactEffect: MonoBehaviour {
    public GameObject effect;

    void AnimationImpact() {
        Instantiate(effect, transform.position, transform.rotation);
    }
}
