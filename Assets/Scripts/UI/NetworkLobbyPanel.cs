using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class NetworkLobbyPanel: MonoBehaviour, IComSatListener {
    public Button readyButton = null;
    public Text readyButtonText = null;
    public Button disconnectButton = null;
    public Text disconnectButtonText = null;

    void Start() {
        ComSat.instance.AddListener(this);
        readyButton.onClick.AddListener(ClickReady);
        disconnectButton.onClick.AddListener(ClickDisconnect);
        gameObject.SetActive(false);
    }

    public void ComSatConnectionStateChanged(ComSat.ConnectionState newState) {
        if(newState == ComSat.ConnectionState.Connected) {
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }

    void Update() {
        if(ComSat.instance.isHost) {
            readyButton.interactable = ComSat.instance.PlayersAreReady();
            readyButtonText.text = "Start Game";
            disconnectButtonText.text = "Shutdown Server";
        } else {
            readyButton.interactable = true;
            readyButtonText.text = "Ready";
            disconnectButtonText.text = "Disconnect";
        }
    }

    void ClickReady() {
        if(ComSat.instance.isHost) {
            ComSat.instance.StartGame();
        } else {
            ComSat.instance.ToggleReady();
        }
    }

    void ClickDisconnect() {
        ComSat.instance.Disconnect();
    }

    void OnDestroy() {
        ComSat.instance.RemoveListener(this);
    }
}
