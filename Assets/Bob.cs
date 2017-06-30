using UnityEngine;

class Bob: MonoBehaviour {
    public Vector3 amplitude = Vector3.zero;
    public Vector3 frequency = Vector3.zero;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotAmplitude = Vector3.zero;
    public Vector3 rotFrequency = Vector3.zero;
    public Vector3 rotOffset = Vector3.zero;

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
        var t1 = rotFrequency * Time.time;
        var t2 = new Vector3(Mathf.Sin(t1.x + rotOffset.x * Mathf.Deg2Rad),
                             Mathf.Sin(t1.y + rotOffset.y * Mathf.Deg2Rad),
                             Mathf.Sin(t1.z + rotOffset.z * Mathf.Deg2Rad));
        transform.localEulerAngles = new Vector3(t2.x * rotAmplitude.x,
                                                 t2.y * rotAmplitude.y,
                                                 t2.z * rotAmplitude.z);
    }
}
