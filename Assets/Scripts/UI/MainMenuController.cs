using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Mapping")]
    [SerializeField] private string gamePlayScene = "Pinewatch Trail";

    [SerializeField] private SessionInitializer sessionInitializer;

    private UIDocument uiDocument;

    private VisualElement mainMenuPanel;
    private VisualElement settingsPanel;

    private SettingsController settingsController;

    private Button continueBtn;
    private Button newGameBtn;
    private Button settingsBtn;
    private Button exitBtn;
    private Button backBtn;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        settingsController = GetComponent<SettingsController>();
        AssignUIReferences();

        // Music and other cross-scene services must exist while the Main Menu
        // is open, not only after New Game or Continue is pressed.
        sessionInitializer?.CreateSession();
    }

    private void OnEnable()
    {
        if (continueBtn != null) continueBtn.clicked += OnContinuePressed;
        if (newGameBtn != null) newGameBtn.clicked += OnNewGamePressed;
        if (settingsBtn != null) settingsBtn.clicked += OpenSettings;
        if (exitBtn != null) exitBtn.clicked += CloseGame;
        if (backBtn != null) backBtn.clicked += CloseSettings;
    }

    private void OnDisable()
    {
        if (continueBtn != null) continueBtn.clicked -= OnContinuePressed;
        if (newGameBtn != null) newGameBtn.clicked -= OnNewGamePressed;
        if (settingsBtn != null) settingsBtn.clicked -= OpenSettings;
        if (exitBtn != null) exitBtn.clicked -= CloseGame;
        if (backBtn != null) backBtn.clicked -= CloseSettings;
    }

    private void AssignUIReferences()
    {
        VisualElement root = uiDocument.rootVisualElement;

        mainMenuPanel = root.Q<VisualElement>("MainMenu");
        settingsPanel = root.Q<VisualElement>("SettingsRoot");

        continueBtn = root.Q<Button>("ContinueButton");
        newGameBtn = root.Q<Button>("NewGameButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        exitBtn = root.Q<Button>("ExitButton");

        backBtn = root.Q<Button>("BackButton");

        if (backBtn == null)
            backBtn = root.Q<Button>("Back-Button");
    }

    private void OnContinuePressed()
    {
        AudioManager.Instance?.PlayUI("Button3");

        sessionInitializer.CreateSession();
        LoadGameplayScene();
    }

    private void OnNewGamePressed()
    {
        AudioManager.Instance?.PlayUI("Button3");

        Debug.Log("MAIN MENU: New Game");

        sessionInitializer.CreateSession();

        Debug.Log("TutorialManager = " + TutorialManager.Instance);

        TutorialManager.Instance.StartTutorial();

        SceneLoader.LoadScene(
            gamePlayScene,
            "ToPinewatchTrail"
        );
    }

    private void OpenSettings()
    {
        AudioManager.Instance?.PlayUI("Button3");

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.None;

        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.Flex;
        else
            Debug.LogError("SettingsRoot not found.");
    }

    private void CloseSettings()
    {
        AudioManager.Instance?.PlayUI("Button3");

        settingsController?.RevertToSavedSettings();

        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.None;

        if (mainMenuPanel != null)
            mainMenuPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseGame()
    {
        AudioManager.Instance?.PlayUI("Button3");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadGameplayScene()
    {
        SceneLoader.LoadScene(gamePlayScene);
    }
}
