using UnityEngine;
using UnityEngine.UI;

class GameMenu: MonoBehaviour {
    public Button resumeButton = null;
    public Button mainMenuButton = null;
    public Button quitButton = null;

    public GameObject menuObject = null;

    void Start() {
        resumeButton.onClick.AddListener(ClickResume);
        mainMenuButton.onClick.AddListener(ClickMainMenu);
        quitButton.onClick.AddListener(ClickQuit);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            menuObject.SetActive(!menuObject.activeSelf);
        }
    }

    void ClickResume() {
        menuObject.SetActive(false);
    }

    void ClickMainMenu() {
        ComSat.instance.Disconnect();
    }

    void ClickQuit() {
        Application.Quit();
    }
}
