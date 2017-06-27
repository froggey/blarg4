using UnityEngine;
using System.Collections.Generic;

class KeyboardMove: MonoBehaviour {
    public float speed = 10.0f;
    public float rotationSpeed = 1.0f;
    public float zoomSpeed = 1.0f;

    public int nCameras = 1;
    public List<Camera> cameras = new List<Camera>();
    public Camera cameraPrefab = null;

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

    public void ReinitializeCameras(int width, int depth) {
        foreach(var cam in cameras) {
            if(cam == Camera.main) {
                continue;
            }
            GameObject.Destroy(cam.gameObject);
        }
        cameras.Clear();

        cameras.Add(Camera.main);

        // Spawn a square of cameras, each offset by some multiple of the terrain width/height.
        for(int x = -nCameras; x <= nCameras; x += 1) {
            for(int y = -nCameras; y <= nCameras; y += 1) {
                if(x == 0 && y == 0) { // main camera
                    continue;
                }
                var cam = (Camera)Instantiate(cameraPrefab, transform.position + new Vector3(x * width, 0, y * depth), Quaternion.identity);
                cam.transform.parent = transform;
                cameras.Add(cam);
            }
        }
    }

    void Start() {
        rotationHelper = new GameObject();

        if(nCameras < 0) {
            nCameras = 0;
        }

        var width = 1024;//(int)Terrain.activeTerrain.terrainData.size.x;
        var depth = 1024;//(int)Terrain.activeTerrain.terrainData.size.z;

        ReinitializeCameras(width, depth);
    }

    // Raycast from the screen, taking wrapped rendering into account.
    public bool Raycast(Vector3 screenPosition, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers) {
        // Fire a ray from each camera, one should eventually hit.
        foreach(var cam in cameras) {
            if(Physics.Raycast(cam.ScreenPointToRay(screenPosition), out hitInfo, maxDistance, layerMask)) {
                return true;
            }
        }
        hitInfo = new RaycastHit();
        return false;
    }

    void Update() {
        /*
        if(Input.GetKey(KeyCode.LeftControl)) {
            currentZoom += Time.deltaTime * zoomSpeed;
        }
        if(Input.GetKey(KeyCode.LeftShift)) {
            currentZoom -= Time.deltaTime * zoomSpeed;
        }
        */
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
        foreach(var cam in cameras) {
            cam.transform.eulerAngles = new Vector3(currentAngle, currentRotation, 0.0f);
            cam.fieldOfView = currentFov;
        }

        // Wrap to terrain.
        var twidth = Terrain.activeTerrain.terrainData.size.x;
        var tdepth = Terrain.activeTerrain.terrainData.size.z;
        var x = Mathf.Repeat(transform.position.x + twidth/2, twidth) - twidth/2;
        var z = Mathf.Repeat(transform.position.z + tdepth/2, tdepth) - tdepth/2;

        //var y = Mathf.Max(heightCurve.Evaluate(currentZoom), Terrain.activeTerrain.SampleHeight(new Vector3(x,0,z)) + 15.0f);
        var y = 140;
        transform.position = new Vector3(x, y, z);
    }
}
