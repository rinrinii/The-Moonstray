using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private VisualElement pauseContainer;
    private VisualElement hudContainer;

    [SerializeField]
    private string mainMenuScene = "MainMenu";

    private Button resumeBtn;
    private Button saveBtn;
    private Button settingsBtn;
    private Button exitBtn;

    private bool isPaused = false;

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
        // Bind to elements safely initialized inside GameplayUIManager
        var ui = GameplayUIManager.Instance;
        pauseContainer = ui.PauseContainer;
        hudContainer = ui.HudContainer;

        VisualElement root = ui.RootVisualElement;
        resumeBtn = root.Q<Button>("ResumeButton");
        saveBtn = root.Q<Button>("SaveButton");
        settingsBtn = root.Q<Button>("SettingsButton");
        exitBtn = root.Q<Button>("ExitButton");

        HookEventSubscriptions();
    }

    private void HookEventSubscriptions()
    {
        if (resumeBtn != null) resumeBtn.clicked += ResumeGame;
        if (saveBtn != null) saveBtn.clicked += SaveGameProgress;
        if (settingsBtn != null) settingsBtn.clicked += OpenOptionsOverlay;
        if (exitBtn != null) exitBtn.clicked += ReturnToTitleScreen;
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

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the map or journal is active, let them process escape closing logic first
            if (GameplayUIManager.Instance.Map.IsMapActive() || GameplayUIManager.Instance.Journal.IsJournalActive())
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
        if (isPaused) PauseGame();
        else ResumeGame();
    }

    private void PauseGame()
    {
        if (pauseContainer != null) pauseContainer.style.display = DisplayStyle.Flex;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.None;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseContainer != null) pauseContainer.style.display = DisplayStyle.None;
        if (hudContainer != null) hudContainer.style.display = DisplayStyle.Flex;
        Time.timeScale = 1f;
    }

    private void SaveGameProgress() => Debug.Log("Saving game progress...");
    private void OpenOptionsOverlay() => Debug.Log("Opening settings menu...");

    private void ReturnToTitleScreen()
    {
        Time.timeScale = 1f;

        PersistentRoot.DestroyPersistentSystems();

        SceneLoader.LoadScene(mainMenuScene);
    }

    public bool IsPaused() => isPaused;
}