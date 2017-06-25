using UnityEngine;
using System.Collections.Generic;

class PlayerInterface: MonoBehaviour {
    KeyboardMove cameraSet = null;
    Game.Navigation navMesh = null;
    Testshit testshit = null;
    ListController uiManager = null;

    public RectTransform screenCanvas = null;

    void Start() {
        cameraSet = Object.FindObjectOfType<KeyboardMove>();
        testshit = Object.FindObjectOfType<Testshit>();
        navMesh = Object.FindObjectOfType<Testshit>().navi;
        uiManager = Object.FindObjectOfType<ListController>();
    }

    GameObject currentlySelected = null;

    Game.DReal timeStep = (Game.DReal)1 / 25;
    public Game.DReal projSpeed = 10;

    void DoFire(Game.DVector3 targetPoint, Game.DVector3 origin) {
        Debug.Log("Firing at " + targetPoint + " from " + origin);
        var dist = navMesh.map.Distance(new Game.DVector3((Game.DReal)origin.x, 0, (Game.DReal)origin.z),
                                        new Game.DVector3((Game.DReal)targetPoint.x, 0, (Game.DReal)targetPoint.z));
        var dir = navMesh.map.Direction(new Game.DVector3((Game.DReal)origin.x, 0, (Game.DReal)origin.z),
                                        new Game.DVector3((Game.DReal)targetPoint.x, 0, (Game.DReal)targetPoint.z));
        Debug.Log("Distance to target is " + dist);
        Debug.DrawRay((Vector3)origin, (Vector3)(dir*dist), Color.white, 10.0f);
        // Walk the line to check for collisions against the terrain.
        // ### Walking the projectile's whole path feels dubious for performance...
        // At least it only needs to be done once. Line/terrain intersect algorithms?
        // This computes X, the highest point of the arc (actually triangle) to clear the terrain.
        //        X
        //       / \
        //      /   \
        //     /    t\
        //    /    tt \
        //   /t   ttt  \
        //  / t  tttt t \
        // / tttttttttt  \
        // Actual rendering uses an arc that passes through X to look nice, and the projectile travel
        // time is computed as though the line was flat.
        var position = origin;
        var midpoint = dist / 2;
        var angle = (Game.DReal)0;
        var intesects_terrain = false;
        for(var offset = (Game.DReal)0; offset < dist; offset += projSpeed * timeStep) {
            var old = position;
            var next_offset  = offset + projSpeed * timeStep;
            if(next_offset >= dist) {
                break;
            }
            position += dir * projSpeed * timeStep;
            var terrain_height = navMesh.map.Height(position);
            if(terrain_height > position.y) {
                Debug.DrawLine((Vector3)old,
                               (Vector3)position,
                               Color.red, 10.0f, false);
                intesects_terrain = true;
                if(offset > midpoint) {
                    angle = Game.DReal.Max(angle, Game.DReal.Atan2(terrain_height - position.y + 1, dist - offset));
                } else {
                    angle = Game.DReal.Max(angle, Game.DReal.Atan2(terrain_height - position.y + 1, offset));
                }
            }
        }
        if(intesects_terrain) {
            Debug.Log("Intersect angle: " + Game.DReal.Degrees(angle) + "  cos: " + Game.DReal.Cos(angle));
            var foo = dir * midpoint;
            var leg = Game.DReal.Half * dist * ((Game.DReal)1 / Game.DReal.Cos(angle));
            var height = Game.DReal.Sqrt((4 * leg * leg - dist * dist) / 4);
            Debug.Log("Height: " + height);
            var middle = new Game.DVector3(foo.x, foo.y + height, foo.z);
            Debug.DrawLine((Vector3)origin, (Vector3)(origin + middle), Color.blue, 10.0f, false);
            Debug.DrawLine((Vector3)(origin + middle), (Vector3)targetPoint, Color.blue, 10.0f, false);
        }
    }

    bool IsMouseOverUiElement() {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public GameObject placement_go;
    public System.Action<Vector3> placement_action_cb;
    public System.Func<Vector3, bool> placement_valid_cb;
    public System.Action placement_cancel_cb;

    void Update() {
        if(placement_go != null) {
            RaycastHit hit;
            var hit_ok = cameraSet.Raycast(Input.mousePosition, out hit, Mathf.Infinity, 1<<9); // terrain only
            if(!IsMouseOverUiElement()) {
                if(Input.GetMouseButtonDown(1)) {
                    Destroy(placement_go);
                    placement_cancel_cb();
                    placement_go = null;
                    return;
                }
                if(Input.GetMouseButtonDown(0) && hit_ok && placement_valid_cb(hit.point)) {
                    Destroy(placement_go);
                    placement_action_cb(hit.point);
                    return;
                }
            }
            if(hit_ok) {
                placement_go.transform.position = hit.point;
                if(placement_valid_cb(hit.point)) {
                    foreach(var mr in placement_go.GetComponentsInChildren<Renderer>()) {
                        mr.material.color = Color.green;
                    }
                } else {
                    foreach(var mr in placement_go.GetComponentsInChildren<Renderer>()) {
                        mr.material.color = Color.red;
                    }
                }
            }
            return;
        }

        if(Input.GetMouseButtonDown(0) && !IsMouseOverUiElement()) {
            RaycastHit hit;
            if(cameraSet.Raycast(Input.mousePosition, out hit)) {
                var next = hit.collider.GetComponentInParent<Selectable>();
                if(next == null) {
                    if(currentlySelected != null) {
                        currentlySelected.SendMessage("OnDeselect");
                        currentlySelected = null;
                        uiManager.ChangeSelected(null);
                    }
                } else {
                    if(next.gameObject != currentlySelected) {
                        if(currentlySelected != null) {
                            currentlySelected.SendMessage("OnDeselect");
                        }
                        next.gameObject.SendMessage("OnSelect");
                        currentlySelected = next.gameObject;
                        uiManager.ChangeSelected(currentlySelected);
                    }
                }
            }
        }

        if(Input.GetMouseButtonDown(1) && !IsMouseOverUiElement()) {
            RaycastHit hit;
            if(currentlySelected != null && cameraSet.Raycast(Input.mousePosition, out hit)) {
                var mirror = currentlySelected.GetComponent<UnityInterwork.EntityMirror>();
                if(mirror != null) {
                    var hit_mirror = hit.collider.GetComponentInParent<UnityInterwork.EntityMirror>();
                    if(hit_mirror == null) {
                        testshit.MoveCommand(mirror, hit.point);
                    } else {
                        testshit.AttackCommand(mirror, hit_mirror);
                    }
                }
            }
        }
    }
}
