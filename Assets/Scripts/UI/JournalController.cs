using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class JournalController : MonoBehaviour
{
    public static JournalController Instance;

    [SerializeField] private VisualTreeAsset journalEntryTemplate;

    private VisualElement journalContainer;
    private bool isJournalOpen = false;

    private VisualElement mainQuestListContainer;
    private VisualElement sideQuestListContainer;

    private VisualElement noteListContainer;
    private Label noteNameLabel;
    private Label noteDescriptionLabel;

    private Label questTitleLabel;
    private Label questDetailsLabel;
    private Label questConditionLabel;
    private Button closeButton;

    private readonly List<JournalNote> notes = new();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void InitializeTemplateBindings(VisualElement root)
    {
        mainQuestListContainer = root.Q<VisualElement>("MainQuestList-Container");
        sideQuestListContainer = root.Q<VisualElement>("SideQuestList-Container");

        questTitleLabel = root.Q<Label>("QuestTitle");
        questDetailsLabel = root.Q<Label>("QuestDetails");
        questConditionLabel = root.Q<Label>("QuestCondition");

        noteListContainer = root.Q<VisualElement>("NoteList-Container");
        noteNameLabel = root.Q<Label>("NoteNameLabel");
        noteDescriptionLabel = root.Q<Label>("NoteDescription");

        closeButton = root.Q<Button>("CloseButton");

        if (closeButton != null)
        {
            closeButton.pickingMode = PickingMode.Position;
            closeButton.clicked += CloseJournal;
        }

        ClearMockElements();
        RenderNotes();
    }

    private void ClearMockElements()
    {
        mainQuestListContainer?.Clear();
        sideQuestListContainer?.Clear();
    }

    private void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
        {
            if (isJournalOpen) CloseJournal();
            return;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            if (isJournalOpen)
                CloseJournal();

            return;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isJournalOpen) CloseJournal();
            else OpenJournal();
        }
    }

    public void OpenJournal()
    {
        if (journalContainer == null) return;

        GameplayUIManager.Instance.SuppressSecondaryPanels(this);

        isJournalOpen = true;
        journalContainer.style.display = DisplayStyle.Flex;
        journalContainer.pickingMode = PickingMode.Position;

        RenderActiveJournalData();
        RenderNotes();
    }

    public void CloseJournal()
    {
        if (journalContainer == null) return;

        isJournalOpen = false;
        journalContainer.style.display = DisplayStyle.None;
    }

    public void AddNote(string title, string content)
    {
        foreach (var note in notes)
        {
            if (note.title == title)
                return;
        }

        notes.Add(new JournalNote(title, content));
        RenderNotes();
    }

    private void RenderNotes()
    {
        if (noteListContainer == null) return;

        noteListContainer.Clear();

        foreach (var note in notes)
        {
            VisualElement entry = CreateJournalEntry(note.title, () =>
            {
                if (noteNameLabel != null) noteNameLabel.text = note.title;
                if (noteDescriptionLabel != null) noteDescriptionLabel.text = note.content;
            });

            noteListContainer.Add(entry);
        }
    }

    private void RenderActiveJournalData()
    {
        ClearMockElements();

        for (int i = 1; i <= 3; i++)
        {
            string questTitle = $"Blight Investigation Task #{i}";
            string abstractSummary = $"Details for step {i}: Cleanse the remaining hazard metrics scattered across the sandbox quadrants.";
            string goalCondition = $"Status: 0 / {i} Cleansed";

            VisualElement entry = CreateJournalEntry(questTitle, () =>
            {
                PopulateFocusedQuestView(questTitle, abstractSummary, goalCondition);
            });

            mainQuestListContainer?.Add(entry);
        }
    }

    private VisualElement CreateJournalEntry(string title, System.Action onClick)
    {
        if (journalEntryTemplate == null)
        {
            Button fallbackButton = new Button();
            fallbackButton.text = title;
            fallbackButton.clicked += () => onClick?.Invoke();
            return fallbackButton;
        }

        TemplateContainer instance = journalEntryTemplate.Instantiate();

        VisualElement root = instance.Q<VisualElement>("JournalEntry");

        if (root == null)
            root = instance.ElementAt(0);

        Button entryButton = root.Q<Button>("EntryTitle");

        if (entryButton != null)
        {
            entryButton.text = title;
            entryButton.clicked += () => onClick?.Invoke();
            entryButton.pickingMode = PickingMode.Position;
        }
        else
        {
            root.RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
        }

        root.pickingMode = PickingMode.Position;

        return root;
    }

    private void PopulateFocusedQuestView(string title, string details, string condition)
    {
        if (questTitleLabel != null) questTitleLabel.text = title;
        if (questDetailsLabel != null) questDetailsLabel.text = details;
        if (questConditionLabel != null) questConditionLabel.text = condition;
    }

    public bool IsJournalActive()
    {
        return journalContainer != null &&
               journalContainer.style.display == DisplayStyle.Flex;
    }

    private class JournalNote
    {
        public string title;
        public string content;

        public JournalNote(string title, string content)
        {
            this.title = title;
            this.content = content;
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        var ui = GameplayUIManager.Instance;

        if (ui == null)
            return;

        journalContainer = ui.JournalContainer;

        if (journalContainer == null)
            return;

        InitializeTemplateBindings(journalContainer);

        CloseJournal();
    }
}