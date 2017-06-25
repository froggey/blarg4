using UnityEngine;

class Smaaash: MonoBehaviour {
    public AnimationCurve curve;
    public float duration;

    public float remaining;

    public Transform animatee;

    void AnimationFire() {
        remaining = duration;
    }

    void Update() {
        if(remaining > 0) {
            animatee.localEulerAngles = new Vector3(curve.Evaluate(duration - remaining),
                                                    animatee.localEulerAngles.y,
                                                    animatee.localEulerAngles.z);
            remaining -= Time.deltaTime;
        }
    }
}
