using UnityEngine;

class Lightning: MonoBehaviour {
    public Vector3 origin;
    public Vector3 target;

    public Vector3 randomMult;
    public int segmentLength = 1;

    LineRenderer[] lineRenderers;

    void Start() {
        lineRenderers = GetComponentsInChildren<LineRenderer>();
    }

    void Update() {
        var dist = (target - origin).magnitude;
        var dir = (target - origin).normalized;
        var n_segs = (int)dist / segmentLength;
        if(n_segs < 2) {
            n_segs = 2;
        }
        var segments = new Vector3[n_segs];
        segments[0] = origin;
        segments[n_segs-1] = target;
        foreach(var lr in lineRenderers) {
            for(var i = 1; i < n_segs-1; i += 1) {
                segments[i] = origin + dir * i * segmentLength + new Vector3(randomMult.x * Random.Range(-1.0f,1.0f),
                                                                             randomMult.y * Random.Range(-1.0f,1.0f),
                                                                             randomMult.z * Random.Range(-1.0f,1.0f));
            }
            lr.numPositions = n_segs;
            lr.SetPositions(segments);
        }
    }
}
