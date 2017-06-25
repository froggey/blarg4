using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ListItemController: MonoBehaviour, IPointerClickHandler {
    public Image greyOut;
    public Image progressBar;
    public Image icon;
    public Text text;
    public System.Action<bool> callback;

    bool uiEnabled = true;

    public void SetUiEnabled(bool state) {
        uiEnabled = state;
        greyOut.gameObject.SetActive(!uiEnabled);
    }

    public void SetProgress(float progress) {
        progressBar.fillAmount = progress;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(uiEnabled) {
            callback(eventData.button == PointerEventData.InputButton.Right);
        }
    }
}
