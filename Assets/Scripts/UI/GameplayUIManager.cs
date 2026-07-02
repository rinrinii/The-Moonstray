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
    public VisualElement InventoryRoot { get; private set; }
    public VisualElement NotePopupRoot { get; private set; }

    // UI Controllers
    public PromptUI Prompt { get; private set; }
    public ObjectivesUI Objectives { get; private set; }

    // Public Sub-Controller properties 
    public MapUI Map { get; private set; }
    public JournalController Journal { get; private set; }
    public InventoryUI Inventory { get; private set; }
    public NoteUI Note { get; private set; }

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
        InventoryRoot = RootVisualElement.Q<VisualElement>("InventoryRoot");
        NotePopupRoot = RootVisualElement.Q<VisualElement>("NotePopupRoot");

        // Grab component references sitting on this same GameObject
        Map = GetComponent<MapUI>();
        Journal = GetComponent<JournalController>();
        Inventory = GetComponent<InventoryUI>();
        Note = GetComponent<NoteUI>();

        Prompt = GetComponent<PromptUI>();
        Objectives = GetComponent<ObjectivesUI>();

        if (Prompt == null)
            Debug.LogWarning("PromptUI component not found.");

        if (Objectives == null)
            Debug.LogWarning("ObjectivesUI component not found.");

        Prompt?.Initialize(RootVisualElement);
        Objectives?.Initialize(RootVisualElement);

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
        if (InventoryRoot != null) InventoryRoot.style.display = DisplayStyle.None;
        if (NotePopupRoot != null) NotePopupRoot.style.display = DisplayStyle.None;
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

}