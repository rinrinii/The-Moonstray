using UnityEngine;
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

    // Public Sub-Controller properties (Fixes the PauseMenuController compile errors!)
    public MapUI Map { get; private set; }
    public JournalController Journal { get; private set; }

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

        // Route structural elements out of your centralized hierarchy
        HudContainer = RootVisualElement.Q<VisualElement>("HUDContainer");
        DialogueContainer = RootVisualElement.Q<VisualElement>("DialogueContainer");
        GameOverContainer = RootVisualElement.Q<VisualElement>("GameOverContainer");
        PauseContainer = RootVisualElement.Q<VisualElement>("PauseContainer");
        JournalContainer = RootVisualElement.Q<VisualElement>("JournalContainer");
        MapRoot = RootVisualElement.Q<VisualElement>("MapRoot");

        // Grab component references sitting on this same GameObject
        Map = GetComponent<MapUI>();
        Journal = GetComponent<JournalController>();

        NormalizeDefaultScreenLayout();
    }

    private void NormalizeDefaultScreenLayout()
    {
        if (HudContainer != null) HudContainer.style.display = DisplayStyle.Flex;
        if (DialogueContainer != null) DialogueContainer.style.display = DisplayStyle.None;
        if (GameOverContainer != null) GameOverContainer.style.display = DisplayStyle.None;
        if (PauseContainer != null) PauseContainer.style.display = DisplayStyle.None;
        if (JournalContainer != null) JournalContainer.style.display = DisplayStyle.None;
        if (MapRoot != null) MapRoot.style.display = DisplayStyle.None;
    }

    public void SuppressSecondaryPanels()
    {
        if (Map != null) Map.CloseMap();
        if (Journal != null) Journal.CloseJournal();
    }
}