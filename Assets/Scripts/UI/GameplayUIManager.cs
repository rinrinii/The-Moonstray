using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance { get; private set; }

    private UIDocument uiDocument;
    public VisualElement RootVisualElement { get; private set; }

    // Core layout container segments (Publicly accessible)
    public VisualElement HudContainer { get; private set; }
    public VisualElement DialogueContainer { get; private set; }
    public VisualElement GameOverContainer { get; private set; }
    public VisualElement PauseContainer { get; private set; }
    public VisualElement JournalContainer { get; private set; }
    public VisualElement MapRoot { get; private set; }
    public VisualElement InventoryRoot { get; private set; }
    public VisualElement NotePopupRoot { get; private set; }
    public VisualElement SettingsRoot { get; private set; }
    public VisualElement EndingChoiceContainer { get; private set; }
    public VisualElement SceneTitleContainer { get; private set; }

    public Button RestoreButton { get; private set; }
    public Button DestroyButton { get; private set; }

    // HUD Sections
    public VisualElement ObjectivesPanel { get; private set; }
    public VisualElement TopRightHUD { get; private set; }
    public VisualElement BottomLeftHUD { get; private set; }
    public VisualElement BottomRightHUD { get; private set; }

    // Frequently accessed HUD elements
    public VisualElement PlayerFormIcon { get; private set; }
    public VisualElement StaminaFill { get; private set; }
    public VisualElement HealthFill { get; private set; }
    public VisualElement HPOverlay { get; private set; }

    // UI Controllers
    public PromptUI Prompt { get; private set; }
    public ObjectivesUI Objectives { get; private set; }

    private Label sceneTitleHeader;
    private Label sceneTitleDescription;
    private Coroutine sceneTitleRoutine;
    private bool sceneTitlesUnlocked;
    private bool tutorialCompleteTitleShown;

    [Header("Scene Title Panel")]
    [SerializeField]
    private float sceneTitleInitialDelay = 0.75f;

    [SerializeField]
    private float sceneTitleVisibleSeconds = 3f;

    // Public Sub-Controller properties 
    public MapUI Map { get; private set; }
    public JournalController Journal { get; private set; }
    public InventoryUI Inventory { get; private set; }
    public NoteUI Note { get; private set; }
    public GameOverUI GameOver { get; private set; }
    public QuestBoardController QuestBoard { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiDocument = GetComponent<UIDocument>();
        RootVisualElement = uiDocument.rootVisualElement;

        // =========================================
        // HUD
        // =========================================

        ObjectivesPanel = RootVisualElement.Q<VisualElement>("ObjectivesPanel");
        TopRightHUD = RootVisualElement.Q<VisualElement>("TopRightHUD");
        BottomLeftHUD = RootVisualElement.Q<VisualElement>("BottomLeftHUD");
        BottomRightHUD = RootVisualElement.Q<VisualElement>("BottomRightHUD");

        // Frequently updated HUD elements
        PlayerFormIcon = RootVisualElement.Q<VisualElement>("PlayerFormIcon");
        StaminaFill = RootVisualElement.Q<VisualElement>("StaminaFill");
        HealthFill = RootVisualElement.Q<VisualElement>("HealthFill");
        HPOverlay = RootVisualElement.Q<VisualElement>("HPOverlay");

        // Route structural elements out of your centralized hierarchy
        HudContainer = RootVisualElement.Q<VisualElement>("HUDContainer");
        DialogueContainer = RootVisualElement.Q<VisualElement>("DialogueContainer");
        GameOverContainer = RootVisualElement.Q<VisualElement>("GameOverContainer");
        PauseContainer = RootVisualElement.Q<VisualElement>("PauseContainer");
        JournalContainer = RootVisualElement.Q<VisualElement>("JournalContainer");
        MapRoot = RootVisualElement.Q<VisualElement>("MapRoot");
        InventoryRoot = RootVisualElement.Q<VisualElement>("InventoryRoot");
        NotePopupRoot = RootVisualElement.Q<VisualElement>("NotePopupRoot");
        SettingsRoot = RootVisualElement.Q<VisualElement>("SettingsRoot");

        EndingChoiceContainer = RootVisualElement.Q<VisualElement>("EndingChoiceContainer");
        SceneTitleContainer = RootVisualElement.Q<VisualElement>("SceneTitleContainer");
        sceneTitleHeader = RootVisualElement.Q<Label>("SceneTitleHeader");
        sceneTitleDescription = RootVisualElement.Q<Label>("SceneTitleDescription");

        RestoreButton = RootVisualElement.Q<Button>("RestoreButton");
        DestroyButton = RootVisualElement.Q<Button>("DestroyButton");

        // Grab component references sitting on this same GameObject
        Map = GetComponent<MapUI>();
        Journal = GetComponent<JournalController>();
        Inventory = GetComponent<InventoryUI>();
        Note = GetComponent<NoteUI>();
        GameOver = GetComponent<GameOverUI>();
        QuestBoard = GetComponent<QuestBoardController>();

        Prompt = GetComponent<PromptUI>();
        Objectives = GetComponent<ObjectivesUI>();

        if (Prompt == null)
            Debug.LogWarning("PromptUI component not found.");

        if (Objectives == null)
            Debug.LogWarning("ObjectivesUI component not found.");

        Prompt?.Initialize(RootVisualElement);
        Objectives?.Initialize(RootVisualElement);

        NormalizeDefaultScreenLayout();
        UpdateVisibilityForScene(SceneManager.GetActiveScene());
    }

    private void Start()
    {
        QueueSceneTitle(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisibilityForScene(scene);
        QueueSceneTitle(scene);
    }

    private void UpdateVisibilityForScene(Scene scene)
    {
        bool isGameplayScene =
            scene.name != "LoadingScene" &&
            scene.name != "MainMenu";

        SetGameplayUIVisible(isGameplayScene);
    }

    private void NormalizeDefaultScreenLayout()
    {
        if (HudContainer != null) HudContainer.style.display = DisplayStyle.Flex;
        if (DialogueContainer != null) DialogueContainer.style.display = DisplayStyle.None;
        if (GameOverContainer != null) GameOverContainer.style.display = DisplayStyle.None;
        if (PauseContainer != null) PauseContainer.style.display = DisplayStyle.None;
        if (SettingsRoot != null) SettingsRoot.style.display = DisplayStyle.None;
        if (JournalContainer != null) JournalContainer.style.display = DisplayStyle.None;
        if (MapRoot != null) MapRoot.style.display = DisplayStyle.None;
        if (InventoryRoot != null) InventoryRoot.style.display = DisplayStyle.None;
        if (NotePopupRoot != null) NotePopupRoot.style.display = DisplayStyle.None;
        if (EndingChoiceContainer != null)
            EndingChoiceContainer.style.display = DisplayStyle.None;
        HideSceneTitle();
    }

    public void ShowSceneTitle(string header, string description)
    {
        if (SceneTitleContainer == null ||
            sceneTitleHeader == null ||
            sceneTitleDescription == null)
        {
            return;
        }

        sceneTitleHeader.text = header;
        sceneTitleDescription.text = description;

        SceneTitleContainer.style.opacity = 1f;
        SceneTitleContainer.style.display = DisplayStyle.Flex;
    }

    private void QueueSceneTitle(Scene scene)
    {
        if (sceneTitleRoutine != null)
            StopCoroutine(sceneTitleRoutine);

        sceneTitleRoutine = StartCoroutine(ShowSceneTitleAfterSceneReady(scene));
    }

    private IEnumerator ShowSceneTitleAfterSceneReady(Scene scene)
    {
        HideSceneTitle();

        yield return null;
        yield return new WaitForSecondsRealtime(sceneTitleInitialDelay);

        if (!ShouldShowSceneTitle(scene, out string header, out string description))
        {
            sceneTitleRoutine = null;
            yield break;
        }

        ShowSceneTitle(header, description);

        yield return new WaitForSecondsRealtime(sceneTitleVisibleSeconds);

        HideSceneTitle();
        sceneTitleRoutine = null;
    }

    private bool ShouldShowSceneTitle(
        Scene scene,
        out string header,
        out string description)
    {
        header = string.Empty;
        description = string.Empty;

        if (scene.name == "LoadingScene" || scene.name == "MainMenu")
            return false;

        TutorialManager tutorial = TutorialManager.Instance;

        if (!tutorialCompleteTitleShown &&
            scene.name == "Moonveil" &&
            tutorial != null &&
            tutorial.IsTutorialFinished)
        {
            tutorialCompleteTitleShown = true;
            sceneTitlesUnlocked = true;
            header = "Tutorial Complete";
            description = "Begin Your Journey";
            return true;
        }

        if (!sceneTitlesUnlocked &&
            tutorial != null &&
            tutorial.IsTutorialFinished)
        {
            sceneTitlesUnlocked = true;
        }

        if (!sceneTitlesUnlocked)
            return false;

        MapData mapData = FindSceneMap(scene.name);

        header = mapData != null &&
            !string.IsNullOrWhiteSpace(mapData.regionTitle)
                ? mapData.regionTitle
                : scene.name;

        description = scene.name;

        return true;
    }

    private MapData FindSceneMap(string sceneName)
    {
        MapData[] maps = Resources.LoadAll<MapData>("Map");

        foreach (MapData map in maps)
        {
            if (map != null && map.sceneName == sceneName)
                return map;
        }

        return null;
    }

    private void HideSceneTitle()
    {
        if (SceneTitleContainer == null)
            return;

        SceneTitleContainer.style.opacity = 0f;
        SceneTitleContainer.style.display = DisplayStyle.None;
    }

    public void SuppressSecondaryPanels(MonoBehaviour except = null)
    {
        if (Map != null && Map != except)
            Map.CloseMap();

        if (Journal != null && Journal != except)
            Journal.CloseJournal();

        if (Inventory != null && Inventory != except)
            Inventory.CloseInventory();

        if (Note != null && Note != except)
            Note.CloseNote();
    }

    // =========================================
    // HUD VISIBILITY
    // =========================================

    private void SetVisible(VisualElement element, bool visible)
    {
        if (element == null)
            return;

        element.style.display =
            visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetGameplayUIVisible(bool visible)
    {
        SetVisible(RootVisualElement, visible);
    }

    public void SetObjectivesVisible(bool visible)
    {
        SetVisible(ObjectivesPanel, visible);
    }

    public void SetTopRightHUDVisible(bool visible)
    {
        SetVisible(TopRightHUD, visible);
    }

    public void SetBottomLeftHUDVisible(bool visible)
    {
        SetVisible(BottomLeftHUD, visible);
    }

    public void SetBottomRightHUDVisible(bool visible)
    {
        SetVisible(BottomRightHUD, visible);
    }

    // =========================================
    // ENDING CHOICE
    // =========================================

    public void ShowEndingChoice()
    {
        SuppressSecondaryPanels();

        if (EndingChoiceContainer != null)
            EndingChoiceContainer.style.display = DisplayStyle.Flex;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        // disable player movement later.
    }

    public void HideEndingChoice()
    {
        if (EndingChoiceContainer != null)
            EndingChoiceContainer.style.display = DisplayStyle.None;

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
