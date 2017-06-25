using UnityEngine;

class DetachedParticleSystem: MonoBehaviour {
    Transform originalParent;

    public ParticleSystem particleSystem;

    void Start() {
        originalParent = transform.parent;
        transform.parent = null;
    }

    void Update() {
        if(originalParent != null) {
            transform.position = originalParent.position;
            transform.rotation = originalParent.rotation;
        } else {
            particleSystem.Stop();
            if(particleSystem.particleCount == 0) {
                Destroy(gameObject);
            }
        }
    }
}
