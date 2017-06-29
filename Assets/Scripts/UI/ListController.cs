using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ListController: MonoBehaviour {
    public GameObject contentPanel;
    public GameObject listItemPrefab;

    bool haveActuallySelectedSomething = false;
    GameObject currentlySelected = null;

    void Update() {
        if(haveActuallySelectedSomething && !currentlySelected) {
            haveActuallySelectedSomething = false;
            currentlySelected = null;
            ClearUiList();
        }
    }

    List<GameObject> listItems = new List<GameObject>();

    void ClearUiList() {
        foreach(var item in listItems) {
            Destroy(item.gameObject);
        }
        listItems.Clear();
    }

    public ListItemController AddUiElement(Sprite image, string title, System.Action<bool> on_click) {
        var go = Instantiate(listItemPrefab) as GameObject;
        var lic = go.GetComponent<ListItemController>();
        lic.icon.sprite = image;
        lic.text.text = title;
        lic.callback = on_click;
        go.transform.SetParent(contentPanel.transform, false);
        go.transform.localScale = Vector3.one;
        listItems.Add(go);
        go.SetActive(false);
        return lic;
    }
}
