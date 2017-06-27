using UnityEngine;

class Bob: MonoBehaviour {
    public Vector3 amplitude = Vector3.zero;
    public Vector3 frequency = Vector3.zero;
    public float offset = 0.0f;

    Vector3 origin;

    void Start() {
        origin = transform.position;
    }

    void Update() {
        var foo = frequency * (Time.time + offset);
        var bar = new Vector3(Mathf.Sin(foo.x),
                              Mathf.Sin(foo.y),
                              Mathf.Sin(foo.z));
        var baz = new Vector3(bar.x * amplitude.x,
                              bar.y * amplitude.y,
                              bar.z * amplitude.z);
        transform.position = origin + baz;
    }
}
