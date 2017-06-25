using UnityEngine;

class FireParticleSystemKicker: MonoBehaviour {
    public ParticleSystem particleSystem;

    void AnimationFire() {
        particleSystem.Play();
    }
}
