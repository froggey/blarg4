using UnityEngine;

class ParticleSystemAutodestroy: MonoBehaviour {
    public ParticleSystem particleSystem;

    void Update() {
        if(!particleSystem.IsAlive()) {
            Destroy(gameObject);
        }
    }
}
