using UnityEngine;
using System.Collections.Generic;

class HitscanEffect: MonoBehaviour {
    public Vector3 positionAdjust;
    public Game.HitscanWeapon weapon = null;
    public Game.Entity target = null;
    public float duration = 0.0f;
    bool burst;

    public float burstDuration = 5.0f;

    LineRenderer lr;
    ParticleSystem ps;
    Lightning lightning;

    public AnimationCurve widthRamp;
    float elapsedTime = 0;

    void Start() {
        burst = duration < 0;
        lr = GetComponent<LineRenderer>();
        if(lr != null) {
            Destroy(gameObject, burst ? burstDuration : duration);
            UpdateLinePosition();
        }
        ps = GetComponent<ParticleSystem>();
        if(ps != null) {
            if(burst) {
                transform.position = (Vector3)target.position + positionAdjust;
                Destroy(gameObject, ps.duration);
            } else {
                UpdateParticleOriginDirection();
                Destroy(gameObject, duration);
            }
            ps.Play();
        }
        lightning = GetComponentInChildren<Lightning>();
        if(lightning != null) {
            Destroy(gameObject, burst ? burstDuration : duration);
            UpdateLightningPosition();
        }
    }

    void UpdateLinePosition() {
        lr.SetPosition(0, (Vector3)weapon.entity.position + positionAdjust);
        lr.SetPosition(1, (Vector3)target.position + positionAdjust);
    }

    void UpdateLightningPosition() {
        lightning.origin = (Vector3)weapon.entity.position + positionAdjust;
        lightning.target = (Vector3)target.position + positionAdjust;
    }

    void UpdateParticleOriginDirection() {
        transform.position = (Vector3)weapon.entity.position + positionAdjust;
        transform.rotation = Quaternion.LookRotation((Vector3)target.position - (Vector3)weapon.entity.position);
    }

    void Update() {
        elapsedTime += Time.deltaTime;
        if(lr != null) {
            lr.widthMultiplier = widthRamp.Evaluate(elapsedTime);
        }
        if(lightning != null) {
            UpdateLightningPosition();
        }
        if(!burst) {
            if(lr != null) {
                UpdateLinePosition();
            }
            if(ps != null && !burst) {
                UpdateParticleOriginDirection();
            }
        }
    }
}
