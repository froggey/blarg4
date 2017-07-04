using UnityEngine;

class Bob: MonoBehaviour {
    public Vector3 amplitude = Vector3.zero;
    public Vector3 frequency = Vector3.zero;
    public Vector3 offset = Vector3.zero;

    Vector3 origin;

    void Start() {
        origin = transform.localPosition;
    }

    void Update() {
        var foo = frequency * Time.time;
        var bar = new Vector3(Mathf.Sin(foo.x + offset.x * Mathf.Deg2Rad),
                              Mathf.Sin(foo.y + offset.y * Mathf.Deg2Rad),
                              Mathf.Sin(foo.z + offset.z * Mathf.Deg2Rad));
        var baz = new Vector3(bar.x * amplitude.x,
                              bar.y * amplitude.y,
                              bar.z * amplitude.z);
        transform.localPosition = origin + baz;
    }
}
