using UnityEngine;

class MuzzleFlash: MonoBehaviour {
    public GameObject muzzleFlashFront;
    public float duration;

    public float remaining;

    void AnimationFire() {
        remaining = duration;
        muzzleFlashFront.SetActive(true);
    }

    void Update() {
        if(remaining > 0) {
            remaining -= Time.deltaTime;
            if(remaining < 0) {
                muzzleFlashFront.SetActive(false);
            }
        }
    }
}
