using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

class LobbyPlayerPanel: MonoBehaviour {
    public InputField playerName;
    public Text factionButtonText;
    public Button factionButton;
    public Text teamButtonText;
    public Button teamButton;
    public Button kickButton;
    public Text readyText;

    public Player player;

    void Start() {
        playerName.onEndEdit.AddListener(UpdatePlayerName);
        factionButton.onClick.AddListener(ClickFactionButton);
        teamButton.onClick.AddListener(ClickTeamButton);
    }

    void UpdatePlayerName(string text) {
        if(player.localPlayer) {
            Debug.Log("Change player name to " + playerName.text);
            ComSat.instance.localPlayerName = playerName.text;
        } else {
            playerName.text = player.name;
        }
    }

    void ClickFactionButton() {
        var newFaction = player.faction == 1 ? 2 : 1;
        ComSat.instance.SetPlayerFaction(player, newFaction);
    }

    void ClickTeamButton() {
        var newTeam = 0;
        if(player.team == -1) {
            newTeam = 1;
        } else {
            newTeam = player.team + 1;
        }
        if(newTeam > 4) {
            newTeam = -1;
        }
        ComSat.instance.SetPlayerTeam(player, newTeam);
    }

    public void UpdatePlayerState() {
        kickButton.gameObject.SetActive(ComSat.instance.isHost && !player.localPlayer);
        playerName.interactable = player.localPlayer;
        factionButton.interactable = player.localPlayer;
        teamButton.interactable = player.localPlayer;
        playerName.text = player.name;
        if(player.team == -1) {
            teamButtonText.text = "Spc";
        } else {
            teamButtonText.text = player.team.ToString();
        }
        if(player.lobbyReady) {
            readyText.text = "Ready";
            readyText.color = Color.green;
        } else {
            readyText.text = "Unready";
            readyText.color = Color.red;
        }
        factionButton.gameObject.SetActive(player.team != -1);
        switch(player.faction) {
        case 1: factionButtonText.text = "Tech"; break;
        case 2: factionButtonText.text = "Magic"; break;
        default: factionButtonText.text = "Faction " + player.faction; break;
        }
        var colorblock = teamButton.colors;
        switch(player.team) {
        case 1: colorblock.normalColor = Color.green; break;
        case 2: colorblock.normalColor = Color.red; break;
        case 3: colorblock.normalColor = Color.blue; break;
        case 4: colorblock.normalColor = Color.yellow; break;
        default: colorblock.normalColor = Color.white; break;
        }
        colorblock.highlightedColor = colorblock.normalColor;
        colorblock.disabledColor = colorblock.normalColor / 1.6f;
        teamButton.colors = colorblock;
    }
}
