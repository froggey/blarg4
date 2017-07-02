using UnityEngine;
using System.Collections.Generic;

class PlayerInterface: MonoBehaviour {
    Game.Navigation navMesh = null;
    Testshit testshit = null;
    ListController uiManager = null;

    public RectTransform screenCanvas = null;

    void Start() {
        testshit = Object.FindObjectOfType<Testshit>();
        navMesh = Object.FindObjectOfType<Testshit>().navi;
        uiManager = Object.FindObjectOfType<ListController>();
    }

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

    void UpdatePlacement() {
        RaycastHit hit;
        var hit_ok = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1<<9); // terrain only
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
    }

    HashSet<UnityInterwork.EntityMirror> currentlySelected = new HashSet<UnityInterwork.EntityMirror>();

    void ClearSelected() {
        foreach(var s in currentlySelected) {
            if(s != null) {
                s.gameObject.SendMessage("OnDeselect");
            }
        }
        currentlySelected.Clear();
    }

    bool marqueeStartDrag = false;
    bool marqueeActive = false;
    Vector2 marqueeOrigin;
    Rect marqueeRect;
    public RectTransform marqueeTranform;

    void Update() {
        currentlySelected.RemoveWhere(x => x == null || x.entity == null);

        if(placement_go != null) {
            marqueeRect.width = 0;
            marqueeRect.height = 0;
            marqueeActive = false;

            UpdatePlacement();
        } else {
            if(IsMouseOverUiElement()) {
                return;
            }

            var multiselect = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if(Input.GetMouseButtonUp(0)) {
                RaycastHit hit;
                if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                    if(!multiselect) {
                        ClearSelected();
                    }

                    var next = hit.collider.GetComponentInParent<Selectable>();
                    if(next != null) {
                        var mirror = next.GetComponent<UnityInterwork.EntityMirror>();
                        while(mirror.parent != null) {
                            mirror = mirror.parent;
                        }
                        if(currentlySelected.Contains(mirror)) {
                            if(multiselect) {
                                mirror.SendMessage("OnDeselect");
                                currentlySelected.Remove(mirror);
                            }
                        } else {
                            mirror.SendMessage("OnSelect");
                            currentlySelected.Add(mirror);
                        }
                    }
                }
            }

            if(Input.GetMouseButtonUp(1)) {
                RaycastHit hit;
                if(currentlySelected.Count != 0 && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                    var hit_mirror = hit.collider.GetComponentInParent<UnityInterwork.EntityMirror>();
                    foreach(var mirror in currentlySelected) {
                        if(hit_mirror == null) {
                            testshit.MoveCommand(mirror, hit.point);
                        } else {
                            while(hit_mirror.parent != null) {
                                hit_mirror = hit_mirror.parent;
                            }
                            testshit.AttackCommand(mirror, hit_mirror);
                        }
                    }
                }
            }

            if(Input.GetMouseButtonDown(0)) {
                marqueeStartDrag = true;
                marqueeOrigin = Input.mousePosition;
            }

            if(marqueeStartDrag && marqueeOrigin != (Vector2)Input.mousePosition) {
                marqueeActive = true;
            }

            if(Input.GetMouseButtonUp(0) && marqueeActive) {
                var selection = new HashSet<UnityInterwork.EntityMirror>();

                if(!multiselect) {
                    ClearSelected();
                }

                foreach(var unit in GameObject.FindGameObjectsWithTag("MultiSelectableUnit")) {
                    var e = unit.GetComponent<UnityInterwork.EntityMirror>();
                    if(e == null) {
                        Debug.LogWarning("No entity in unit " + unit);
                        continue;
                    }
                    while(e.parent != null) {
                        e = e.parent;
                    }
                    if(selection.Contains(e)) {
                        continue;
                    }
                    if(ComSat.instance != null && e.entity.team != ComSat.instance.localPlayer.team) {
                        continue;
                    }

                    // Convert the world position of the unit to a screen position and then to a GUI point.
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                    Vector2 screenPoint = new Vector2(screenPos.x, screenPos.y);

                    if(marqueeRect.Contains(screenPoint)) {
                        selection.Add(e);
                    }
                }

                foreach(var u in selection) {
                    u.SendMessage("OnSelect");
                    currentlySelected.Add(u);
                }

                // Reset the marquee so it no longer appears on the screen.
                marqueeRect.width = 0;
                marqueeRect.height = 0;
                marqueeActive = false;
            }

            if(!Input.GetMouseButton(0)) {
                marqueeActive = false;
                marqueeStartDrag = false;
            }

            if(marqueeActive) {
                Vector2 mouse = Input.mousePosition;

                // Compute a new marquee rectangle.
                marqueeRect.x = marqueeOrigin.x;
                marqueeRect.y = marqueeOrigin.y;
                marqueeRect.width = mouse.x - marqueeOrigin.x;
                marqueeRect.height = mouse.y - marqueeOrigin.y;

                // Prevent negative widths/heights.
                if(marqueeRect.width < 0) {
                    marqueeRect.x += marqueeRect.width;
                    marqueeRect.width = -marqueeRect.width;
                }
                if(marqueeRect.height < 0) {
                    marqueeRect.y += marqueeRect.height;
                    marqueeRect.height = -marqueeRect.height;
                }
            }
        }
    }

    void LateUpdate() {
        if(marqueeActive) {
            marqueeTranform.gameObject.SetActive(true);
            marqueeTranform.position = new Vector3(marqueeRect.x, marqueeRect.y, 0);
            marqueeTranform.sizeDelta = new Vector2(marqueeRect.width, marqueeRect.height);
        } else {
            marqueeTranform.gameObject.SetActive(false);
        }
    }
}
