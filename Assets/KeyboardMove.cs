using UnityEngine;
using System.Collections.Generic;

class KeyboardMove: MonoBehaviour {
    public float speed = 10.0f;
    public float rotationSpeed = 1.0f;
    public float zoomSpeed = 1.0f;

    public float currentZoom = 0.0f;

    public AnimationCurve angleCurve = null;
    public AnimationCurve fovCurve = null;
    public AnimationCurve heightCurve = null;

    // Driven by zoom and the animation curves.
    public float currentAngle = 45.0f;
    public float currentFov = 30.0f;

    public Vector3 lookPoint;

    void Update() {
        if(Input.GetKey(KeyCode.F)) {
            currentZoom += Time.deltaTime * zoomSpeed;
        }
        if(Input.GetKey(KeyCode.R)) {
            currentZoom -= Time.deltaTime * zoomSpeed;
        }
        currentZoom = Mathf.Clamp01(currentZoom);
        currentAngle = angleCurve.Evaluate(currentZoom);
        currentFov = fovCurve.Evaluate(currentZoom);

        if(Input.GetKey(KeyCode.W)) {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.S)) {
            transform.Translate(-Vector3.forward * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.D)) {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.A)) {
            transform.Translate(-Vector3.right * Time.deltaTime * speed);
        }
        var spin = 0.0f;
        if(Input.GetKey(KeyCode.E)) {
            spin += 1.0f;
        }
        if(Input.GetKey(KeyCode.Q)) {
            spin -= 1.0f;
        }

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, 1<<9)) { // terrain only
            transform.RotateAround(hit.point, Vector3.up, Time.deltaTime * spin * rotationSpeed);
            lookPoint = hit.point;
        }
        Camera.main.transform.localEulerAngles = new Vector3(currentAngle, 0.0f, 0.0f);
        Camera.main.fieldOfView = currentFov;

        // Wrap to terrain.
        var twidth = Terrain.activeTerrain.terrainData.size.x;
        var tdepth = Terrain.activeTerrain.terrainData.size.z;
        var x = Mathf.Repeat(transform.position.x, twidth);
        var z = Mathf.Repeat(transform.position.z, tdepth);

        var y = heightCurve.Evaluate(currentZoom);
        transform.position = new Vector3(x, y, z);
    }
}
