using UnityEngine;
using System.Collections.Generic;

namespace MapEditor {
class CameraMotion: MonoBehaviour {
    public float speed = 10.0f;
    public float rotationSpeed = 1.0f;
    public float zoomSpeed = 1.0f;

    public float currentRotation = 0.0f;
    public float currentZoom = 0.0f;

    public AnimationCurve angleCurve = null;
    public AnimationCurve fovCurve = null;
    public AnimationCurve heightCurve = null;

    // Driven by zoom and the animation curves.
    public float currentAngle = 45.0f;
    public float currentFov = 30.0f;

    // Used to figure out the current forward/right vector for the camera group.
    GameObject rotationHelper;

    void Start() {
        rotationHelper = new GameObject();
    }

    void Update() {
        if(Input.GetKey(KeyCode.LeftControl)) {
            currentZoom += Time.deltaTime * zoomSpeed;
        }
        if(Input.GetKey(KeyCode.LeftShift)) {
            currentZoom -= Time.deltaTime * zoomSpeed;
        }
        currentZoom = Mathf.Clamp01(currentZoom);
        currentAngle = angleCurve.Evaluate(currentZoom);
        currentFov = fovCurve.Evaluate(currentZoom);

        if(Input.GetKey(KeyCode.W)) {
            transform.Translate(rotationHelper.transform.forward * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.S)) {
            transform.Translate(-rotationHelper.transform.forward * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.D)) {
            transform.Translate(rotationHelper.transform.right * Time.deltaTime * speed);
        }
        if(Input.GetKey(KeyCode.A)) {
            transform.Translate(-rotationHelper.transform.right * Time.deltaTime * speed);
        }
        // TODO: Use RotateAround instead to rotate around the point being looked at.
        // Raycasting to find the point won't work as that may miss the terrain...
        var spin = 0.0f;
        if(Input.GetKey(KeyCode.E)) {
            spin += 1.0f;
        }
        if(Input.GetKey(KeyCode.Q)) {
            spin -= 1.0f;
        }

        currentRotation += Time.deltaTime * spin * rotationSpeed;
        currentRotation = Mathf.Repeat(currentRotation, 360.0f);
        rotationHelper.transform.eulerAngles = new Vector3(0.0f, currentRotation, 0.0f);
        Camera.main.transform.eulerAngles = new Vector3(currentAngle, currentRotation, 0.0f);
        Camera.main.fieldOfView = currentFov;

        transform.position = new Vector3(transform.position.x,
                                         heightCurve.Evaluate(currentZoom),
                                         transform.position.z);
    }
}
}