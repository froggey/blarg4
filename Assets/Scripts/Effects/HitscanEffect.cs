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
                Destroy(gameObject, ps.main.duration);
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

        if(lr == null && ps == null && lightning == null) {
            Destroy(gameObject);
        }
    }

    Vector3 CorrectedEntityPosition() {
        return (Vector3)weapon.entity.position + positionAdjust;
    }

    Vector3 CorrectedTargetPosition() {
        var adjTarget = (Vector3)target.position + positionAdjust;
        var diff = adjTarget - CorrectedEntityPosition();
        if(diff.x > 512) {
            diff.x -= 1024;
        } else if(diff.x < -512) {
            diff.x += 1024;
        }
        if(diff.z > 512) {
            diff.z -= 1024;
        } else if(diff.z < -512) {
            diff.z += 1024;
        }
        return CorrectedEntityPosition() + diff;
    }

    void UpdateLinePosition() {
        lr.SetPosition(0, CorrectedEntityPosition());
        lr.SetPosition(1, CorrectedTargetPosition());
    }

    void UpdateLightningPosition() {
        lightning.origin = CorrectedEntityPosition();
        lightning.target = CorrectedTargetPosition();
    }

    void UpdateParticleOriginDirection() {
        transform.position = CorrectedEntityPosition();
        transform.rotation = Quaternion.LookRotation(CorrectedTargetPosition() - CorrectedEntityPosition());
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
