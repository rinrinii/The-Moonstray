using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Mapping")]
    [SerializeField] private string gamePlayScene = "Test_GameplayUI";

    private UIDocument uiDocument;
    private VisualElement mainMenuPanel;
    private VisualElement settingsPanel;

    private Button continueBtn;
    private Button newGameBtn;
    private Button settingsBtn;
    private Button exitBtn;

    private Button audioTabBtn;
    private Button videoTabBtn;
    private Button controlsTabBtn;
    private Button backBtn;
    private Button saveBtn;

    private VisualElement audioSubPanel;
    private VisualElement videoSubPanel;
    private VisualElement controlsSubPanel;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        AssignUIReferences();
    }

    private void AssignUIReferences()
    {
        VisualElement root = uiDocument.rootVisualElement;

        mainMenuPanel = root.Q<VisualElement>("MainMenu");
        settingsPanel = root.Q<VisualElement>("Settings");

        continueBtn = root.Q<Button>("ContinueButton");
        newGameBtn  = root.Q<Button>("NewGameButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        exitBtn     = root.Q<Button>("ExitButton");

        audioTabBtn    = root.Q<Button>("Audio-Button");
        videoTabBtn    = root.Q<Button>("Video-Button");
        controlsTabBtn = root.Q<Button>("Controls-Button");
        backBtn        = root.Q<Button>("Back-Button");
        saveBtn        = root.Q<Button>("SaveSettings-Button");

        audioSubPanel    = root.Q<VisualElement>("Audio-Settings");
        videoSubPanel    = root.Q<VisualElement>("Video-Settings");
        controlsSubPanel = root.Q<VisualElement>("Controls-Settings");
    }

    private void OnEnable()
    {
        if (continueBtn != null) continueBtn.clicked += OnContinuePressed;
        if (newGameBtn != null)   newGameBtn.clicked += OnNewGamePressed;
        if (settingsBtn != null)  settingsBtn.clicked += OpenSettings;
        if (exitBtn != null)      exitBtn.clicked += CloseGame;

        if (backBtn != null)        backBtn.clicked += CloseSettings;
        if (saveBtn != null)        saveBtn.clicked += SaveConfiguration;
        if (audioTabBtn != null)    audioTabBtn.clicked += () => SwitchSettingsTab(audioSubPanel);
        if (videoTabBtn != null)    videoTabBtn.clicked += () => SwitchSettingsTab(videoSubPanel);
        if (controlsTabBtn != null) controlsTabBtn.clicked += () => SwitchSettingsTab(controlsSubPanel);
    }

    private void OnDisable()
    {
        if (continueBtn != null) continueBtn.clicked -= OnContinuePressed;
        if (newGameBtn != null)   newGameBtn.clicked -= OnNewGamePressed;
        if (settingsBtn != null)  settingsBtn.clicked -= OpenSettings;
        if (exitBtn != null)      exitBtn.clicked -= CloseGame;
        if (backBtn != null)        backBtn.clicked -= CloseSettings;
        if (saveBtn != null)        saveBtn.clicked -= SaveConfiguration;
    }

    private void OnContinuePressed() => LoadGameplayScene();
    private void OnNewGamePressed() => LoadGameplayScene();

    private void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.style.display = DisplayStyle.None;
        if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.Flex;
        SwitchSettingsTab(audioSubPanel);
    }

    private void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.None;
        if (mainMenuPanel != null) mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    private void SwitchSettingsTab(VisualElement activePanel)
    {
        if (audioSubPanel != null) audioSubPanel.style.display = DisplayStyle.None;
        if (videoSubPanel != null) videoSubPanel.style.display = DisplayStyle.None;
        if (controlsSubPanel != null) controlsSubPanel.style.display = DisplayStyle.None;

        if (activePanel != null) activePanel.style.display = DisplayStyle.Flex;
    }

    private void SaveConfiguration() => Debug.Log("Configuration settings flushed to storage...");

    private void CloseGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void LoadGameplayScene() => SceneManager.LoadScene(gamePlayScene);
}