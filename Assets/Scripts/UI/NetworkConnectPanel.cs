using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class NetworkConnectPanel: MonoBehaviour, IComSatListener {
    public InputField playerName = null;

    public InputField serverAddress = null;
    public InputField serverPort = null;

    public Button hostButton = null;
    public Button connectButton = null;
    public Button quitButton = null;

    void Start() {
        ComSat.instance.AddListener(this);
        playerName.onEndEdit.AddListener(UpdatePlayerName);
        playerName.text = ComSat.instance.localPlayerName;
        hostButton.onClick.AddListener(DoHost);
        connectButton.onClick.AddListener(DoConnect);
        quitButton.onClick.AddListener(DoQuit);
    }

    void OnDestroy() {
        ComSat.instance.RemoveListener(this);
    }

    public void ComSatConnectionStateChanged(ComSat.ConnectionState newState) {
        if(newState == ComSat.ConnectionState.Disconnected) {
            playerName.text = ComSat.instance.localPlayerName;
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }

    public void ComSatPlayerJoined(Player player) {}
    public void ComSatPlayerChanged(Player player) {}
    public void ComSatPlayerLeft(Player player) {}

    void UpdatePlayerName(string text) {
        Debug.Log("Change player name to " + playerName.text);
        ComSat.instance.localPlayerName = playerName.text;
    }

    void DoHost() {
        ComSat.instance.Host(System.Int32.Parse(serverPort.text));
    }

    void DoConnect() {
        ComSat.instance.Connect(serverAddress.text,
                                System.Int32.Parse(serverPort.text));
    }

    void DoQuit() {
        Application.Quit();
    }
}
