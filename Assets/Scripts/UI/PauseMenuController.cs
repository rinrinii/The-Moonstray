using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private VisualElement pauseContainer;
    private VisualElement pausePanel;
    private VisualElement hudContainer;
    private VisualElement settingsContainer;

    [SerializeField] private string mainMenuScene = "MainMenu";

    private Button resumeBtn;
    private Button saveBtn;
    private Button settingsBtn;
    private Button exitBtn;
    private Button backBtn;

    private SettingsController settingsController;

    private bool isPaused = false;
    private bool settingsOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        var ui = GameplayUIManager.Instance;

        pauseContainer = ui.PauseContainer;
        hudContainer = ui.HudContainer;
        settingsContainer = ui.SettingsRoot;

        VisualElement root = ui.RootVisualElement;

        pausePanel = root.Q<VisualElement>("PausePanel");

        resumeBtn = root.Q<Button>("ResumeButton");
        saveBtn = root.Q<Button>("SaveButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        exitBtn = root.Q<Button>("ExitButton");

        backBtn = root.Q<Button>("BackButton");

        if (backBtn == null)
            backBtn = root.Q<Button>("Back-Button");

        settingsController = GetComponent<SettingsController>();

        HookEventSubscriptions();

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.None;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.Flex;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;
        else
            Debug.LogError("SettingsRoot not found from GameplayUIManager.");
    }

    private void HookEventSubscriptions()
    {
        if (resumeBtn != null)
            resumeBtn.clicked += ResumeGame;

        if (saveBtn != null)
            saveBtn.clicked += SaveGameProgress;

        if (settingsBtn != null)
            settingsBtn.clicked += OpenOptionsOverlay;

        if (exitBtn != null)
            exitBtn.clicked += ReturnToTitleScreen;

        if (backBtn != null)
            backBtn.clicked += CloseOptionsOverlay;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (resumeBtn != null)
            resumeBtn.clicked -= ResumeGame;

        if (saveBtn != null)
            saveBtn.clicked -= SaveGameProgress;

        if (settingsBtn != null)
            settingsBtn.clicked -= OpenOptionsOverlay;

        if (exitBtn != null)
            exitBtn.clicked -= ReturnToTitleScreen;

        if (backBtn != null)
            backBtn.clicked -= CloseOptionsOverlay;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsOpen)
            {
                CloseOptionsOverlay();
                return;
            }

            if (GameplayUIManager.Instance.Map.IsMapActive() ||
                GameplayUIManager.Instance.Journal.IsJournalActive())
            {
                GameplayUIManager.Instance.SuppressSecondaryPanels();
                return;
            }

            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
            PauseGame();
        else
            ResumeGame();
    }

    private void PauseGame()
    {
        isPaused = true;
        settingsOpen = false;

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.Flex;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.Flex;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;

        if (hudContainer != null)
            hudContainer.style.display = DisplayStyle.None;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        AudioManager.Instance?.PlayUI("Button3");

        isPaused = false;
        settingsOpen = false;

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.None;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.Flex;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;

        if (hudContainer != null)
            hudContainer.style.display = DisplayStyle.Flex;

        Time.timeScale = 1f;
    }

    private void SaveGameProgress()
    {
        AudioManager.Instance?.PlayUI("Button3");

        Debug.Log("Saving game progress...");
    }

    private void OpenOptionsOverlay()
    {
        AudioManager.Instance?.PlayUI("Button3");

        settingsOpen = true;

        if (pauseContainer != null)
            pauseContainer.style.display = DisplayStyle.Flex;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.None;

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.Flex;
        else
            Debug.LogError("SettingsRoot not found in PauseMenuVT.");
    }

    private void CloseOptionsOverlay()
    {
        AudioManager.Instance?.PlayUI("Button3");

        settingsOpen = false;

        settingsController?.RevertToSavedSettings();

        if (settingsContainer != null)
            settingsContainer.style.display = DisplayStyle.None;

        if (pausePanel != null)
            pausePanel.style.display = DisplayStyle.Flex;
    }

    private void ReturnToTitleScreen()
    {
        AudioManager.Instance?.PlayUI("Button3");

        Time.timeScale = 1f;

        PersistentRoot.DestroyPersistentSystems();

        SceneLoader.LoadScene(mainMenuScene);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}