using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class NetworkLobbyPanel: MonoBehaviour, IComSatListener {
    public Button startGameButton = null;
    public Button readyButton = null;
    public Text readyButtonText = null;
    public Button disconnectButton = null;
    public Text disconnectButtonText = null;

    public GameObject lobbyPlayerPrefab = null;
    public RectTransform lobbyPlayerList = null;

    Dictionary<Player, LobbyPlayerPanel> playerPanels = new Dictionary<Player, LobbyPlayerPanel>();

    void Start() {
        ComSat.instance.AddListener(this);
        startGameButton.onClick.AddListener(ClickStartGame);
        readyButton.onClick.AddListener(ClickReady);
        disconnectButton.onClick.AddListener(ClickDisconnect);
        gameObject.SetActive(false);
    }

    void OnDestroy() {
        ComSat.instance.RemoveListener(this);
    }

    public void ComSatConnectionStateChanged(ComSat.ConnectionState newState) {
        if(newState == ComSat.ConnectionState.Connected) {
            startGameButton.gameObject.SetActive(ComSat.instance.isHost);
            gameObject.SetActive(true);
        } else {
            gameObject.SetActive(false);
        }
    }

    public void ComSatPlayerJoined(Player player) {
        var go = Instantiate(lobbyPlayerPrefab);
        go.transform.SetParent(lobbyPlayerList, false);
        var lpp = go.GetComponent<LobbyPlayerPanel>();
        lpp.player = player;
        lpp.UpdatePlayerState();
        playerPanels.Add(player, lpp);
    }

    public void ComSatPlayerChanged(Player player) {
        playerPanels[player].UpdatePlayerState();
    }

    public void ComSatPlayerLeft(Player player) {
        var lpp = playerPanels[player];
        playerPanels.Remove(player);
        Destroy(lpp.gameObject);
    }

    void Update() {
        if(ComSat.instance.isHost) {
            startGameButton.interactable = ComSat.instance.PlayersAreLobbyReady();
            disconnectButtonText.text = "Shutdown Server";
        } else {
            disconnectButtonText.text = "Disconnect";
        }
        if(ComSat.instance.localPlayer != null && ComSat.instance.localPlayer.lobbyReady) {
            readyButtonText.text = "Unready";
        } else {
            readyButtonText.text = "Ready";
        }
    }

    void ClickStartGame() {
        ComSat.instance.StartGame();
    }

    void ClickReady() {
        ComSat.instance.ToggleReady();
    }

    void ClickDisconnect() {
        ComSat.instance.Disconnect();
    }
}
