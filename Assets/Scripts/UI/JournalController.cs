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

    private ScrollView noteListContainer;
    private ScrollView noteContentContainer;
    private Label noteNameLabel;
    private Label noteDescriptionLabel;

    private Label questTitleLabel;
    private Label questDetailsLabel;
    private Label questConditionLabel;
    private Label questRewardsHeaderLabel;
    private Label questRewardsLabel;
    private Label questSubmitStatusLabel;
    private ScrollView questDetailsScroll;
    private Button submitQuestButton;
    private Button trackQuestButton;
    private Button closeButton;
    private QuestState selectedQuest;

    [SerializeField]
    private bool unlocked = false;
    public bool IsUnlocked => unlocked;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Unlock();
        }

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            if (isJournalOpen)
                CloseJournal();

            return;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            if (isJournalOpen)
                CloseJournal();

            return;
        }

        if (unlocked && Input.GetKeyDown(KeyCode.J))
        {
            if (isJournalOpen)
                CloseJournal();
            else
                OpenJournal();
        }
    }

    private void InitializeTemplateBindings(VisualElement root)
    {
        mainQuestListContainer =
            root.Q<VisualElement>("MainQuestList-Container");

        sideQuestListContainer =
            root.Q<VisualElement>("SideQuestList-Container");

        questTitleLabel =
            root.Q<Label>("QuestTitle");

        questDetailsLabel =
            root.Q<Label>("QuestDetails");

        questConditionLabel =
            root.Q<Label>("QuestCondition");

        questRewardsHeaderLabel =
            root.Q<Label>("RewardsLabel");

        questRewardsLabel =
            root.Q<Label>("QuestRewards");

        questSubmitStatusLabel =
            root.Q<Label>("QuestSubmitStatus");

        questDetailsScroll =
            root.Q<ScrollView>("QuestDetailsScroll");

        submitQuestButton =
            root.Q<Button>("SubmitQuestButton");

        trackQuestButton =
            root.Q<Button>("TrackQuestButton");

        SetupScrollView(root.Q<ScrollView>("MainQuestList-Container"));
        SetupScrollView(root.Q<ScrollView>("SideQuestList-Container"));
        SetupScrollView(questDetailsScroll);

        noteListContainer =
            root.Q<ScrollView>("NoteList-Container");

        noteContentContainer =
            root.Q<ScrollView>("NoteContentContainer");

        SetupScrollView(noteContentContainer);
        SetupScrollView(noteListContainer);

        noteNameLabel =
            root.Q<Label>("NoteNameLabel");

        noteDescriptionLabel =
            root.Q<Label>("NoteDescription");

        if (noteNameLabel != null)
        {
            noteNameLabel.style.whiteSpace = WhiteSpace.Normal;
            noteNameLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            noteNameLabel.style.alignSelf = Align.Stretch;
        }

        if (noteDescriptionLabel != null)
        {
            noteDescriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            noteDescriptionLabel.style.alignSelf = Align.Stretch;
        }

        closeButton =
            root.Q<Button>("CloseButton");

        if (closeButton != null)
        {
            closeButton.pickingMode = PickingMode.Position;
            closeButton.clicked -= CloseJournal;
            closeButton.clicked += CloseJournal;
        }

        if (submitQuestButton != null)
        {
            submitQuestButton.clicked -= SubmitSelectedQuest;
            submitQuestButton.clicked += SubmitSelectedQuest;
            submitQuestButton.style.display = DisplayStyle.None;
        }

        if (trackQuestButton != null)
        {
            trackQuestButton.clicked -= TrackSelectedQuest;
            trackQuestButton.clicked += TrackSelectedQuest;
            trackQuestButton.style.display = DisplayStyle.None;
        }

        ClearMockElements();
        RenderNotes();
    }

    private void ClearMockElements()
    {
        ClearContainer(mainQuestListContainer);
        ClearContainer(sideQuestListContainer);
    }

    public void OpenJournal()
    {
        if (!unlocked)
            return;

        if (journalContainer == null)
            return;

        GameplayUIManager.Instance.SuppressSecondaryPanels(this);

        isJournalOpen = true;
        journalContainer.style.display = DisplayStyle.Flex;
        journalContainer.pickingMode = PickingMode.Position;

        selectedQuest = null;
        RenderActiveJournalData();
        RenderNotes();
    }

    public void CloseJournal()
    {
        if (journalContainer == null)
            return;

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

    public bool HasNote(NoteData noteData)
    {
        if (noteData == null)
            return false;

        return HasNote(noteData.title);
    }

    public bool HasNote(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        foreach (JournalNote note in notes)
        {
            if (note.title == title)
                return true;
        }

        return false;
    }

    private void RenderNotes()
    {
        if (noteListContainer == null)
            return;

        noteListContainer.contentContainer.Clear();

        foreach (var note in notes)
        {
            VisualElement entry = CreateJournalEntry(note.title, () =>
            {
                if (noteNameLabel != null)
                    noteNameLabel.text = note.title;

                if (noteDescriptionLabel != null)
                    noteDescriptionLabel.text = note.content;

                if (noteContentContainer != null)
                    noteContentContainer.scrollOffset = Vector2.zero;
            });

            noteListContainer.contentContainer.Add(entry);
        }
    }

    private void RenderActiveJournalData()
    {
        ClearMockElements();

        List<QuestState> mainQuests = new();

        QuestState mainQuest = QuestManager.Instance?.CurrentMainQuest;

        if (mainQuest != null)
            mainQuests.Add(mainQuest);

        if (QuestManager.Instance != null)
        {
            foreach (QuestState objectiveQuest in
                     QuestManager.Instance.ObjectiveJournalQuests)
            {
                mainQuests.Add(objectiveQuest);
            }
        }

        mainQuests.Sort((left, right) =>
            right.LastUpdatedOrder.CompareTo(left.LastUpdatedOrder));

        foreach (QuestState quest in mainQuests)
        {
            VisualElement mainQuestEntry = CreateJournalEntry(quest.Title, () =>
            {
                PopulateFocusedQuestView(quest);
            });

            AddToContainer(mainQuestListContainer, mainQuestEntry);
        }

        if (selectedQuest == null && mainQuests.Count > 0)
            PopulateFocusedQuestView(mainQuests[0]);

        if (QuestManager.Instance == null)
            return;

        foreach (QuestState sideQuest in QuestManager.Instance.SideQuests)
        {
            VisualElement entry = CreateJournalEntry(sideQuest.Title, () =>
            {
                PopulateFocusedQuestView(sideQuest);
            });

            AddToContainer(sideQuestListContainer, entry);
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

        TemplateContainer instance =
            journalEntryTemplate.Instantiate();

        VisualElement root =
            instance.Q<VisualElement>("JournalEntry");

        if (root == null)
            root = instance.ElementAt(0);

        Button entryButton =
            root.Q<Button>("EntryTitle");

        if (entryButton != null)
        {
            entryButton.text = title;
            entryButton.clicked += () => onClick?.Invoke();
            entryButton.pickingMode = PickingMode.Position;
            entryButton.style.whiteSpace = WhiteSpace.Normal;
            entryButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            entryButton.style.alignSelf = Align.Stretch;
            entryButton.style.width = Length.Percent(100);
        }
        else
        {
            root.RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
        }

        root.pickingMode = PickingMode.Position;
        root.style.alignSelf = Align.Stretch;
        root.style.width = Length.Percent(100);

        return root;
    }

    private void PopulateFocusedQuestView(
        string title,
        string details,
        string condition)
    {
        selectedQuest = null;

        if (questTitleLabel != null)
            questTitleLabel.text = title;

        if (questDetailsLabel != null)
            questDetailsLabel.text = details;

        if (questConditionLabel != null)
            questConditionLabel.text = condition;

        SetRewardsVisible(false);

        if (questSubmitStatusLabel != null)
            questSubmitStatusLabel.text = string.Empty;

        if (submitQuestButton != null)
            submitQuestButton.style.display = DisplayStyle.None;

        if (trackQuestButton != null)
            trackQuestButton.style.display = DisplayStyle.None;

        if (questDetailsScroll != null)
            questDetailsScroll.scrollOffset = Vector2.zero;
    }

    private void PopulateFocusedQuestView(QuestState quest)
    {
        if (quest == null)
            return;

        selectedQuest = quest;

        if (questTitleLabel != null)
            questTitleLabel.text = quest.Title;

        if (questDetailsLabel != null)
            questDetailsLabel.text = string.IsNullOrWhiteSpace(quest.Description)
                ? "Nothing here..."
                : quest.Description;

        if (questConditionLabel != null)
            questConditionLabel.text = string.IsNullOrWhiteSpace(quest.Conditions)
                ? "Nothing here..."
                : quest.Conditions;

        bool isSideQuest =
            quest.Data != null &&
            quest.Data.category == QuestCategory.Side;

        SetRewardsVisible(isSideQuest);

        if (isSideQuest && questRewardsLabel != null)
        {
            questRewardsLabel.text = string.IsNullOrWhiteSpace(quest.Rewards)
                ? "Nothing here..."
                : quest.Rewards;
        }

        if (questSubmitStatusLabel != null)
            questSubmitStatusLabel.text = string.Empty;

        if (submitQuestButton != null)
        {
            bool canSubmit = isSideQuest && !quest.Completed;
            submitQuestButton.style.display =
                canSubmit ? DisplayStyle.Flex : DisplayStyle.None;
            submitQuestButton.SetEnabled(canSubmit);
        }

        if (trackQuestButton != null)
        {
            trackQuestButton.style.display = DisplayStyle.None;
            trackQuestButton.SetEnabled(false);
        }

        if (questDetailsScroll != null)
            questDetailsScroll.scrollOffset = Vector2.zero;
    }

    private void SetRewardsVisible(bool visible)
    {
        DisplayStyle display =
            visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (questRewardsHeaderLabel != null)
            questRewardsHeaderLabel.style.display = display;

        if (questRewardsLabel != null)
        {
            questRewardsLabel.style.display = display;

            if (!visible)
                questRewardsLabel.text = string.Empty;
        }
    }

    private void SubmitSelectedQuest()
    {
        if (selectedQuest == null || selectedQuest.Data == null)
            return;

        if (QuestManager.Instance == null)
        {
            if (questSubmitStatusLabel != null)
                questSubmitStatusLabel.text = "Quest manager is unavailable.";

            return;
        }

        if (!QuestManager.Instance.SubmitSideQuest(
                selectedQuest.Data,
                out string failureReason))
        {
            if (questSubmitStatusLabel != null)
                questSubmitStatusLabel.text = failureReason;

            return;
        }

        if (questSubmitStatusLabel != null)
            questSubmitStatusLabel.text = "Quest complete.";

        if (submitQuestButton != null)
            submitQuestButton.style.display = DisplayStyle.None;

        selectedQuest = null;
        RenderActiveJournalData();
    }

    private void TrackSelectedQuest()
    {
        if (selectedQuest == null)
            return;

        Debug.Log($"Tracking quest: {selectedQuest.Title}");
    }

    private void SetupScrollView(ScrollView scrollView)
    {
        if (scrollView == null)
            return;

        scrollView.verticalScrollerVisibility =
            ScrollerVisibility.Auto;
        scrollView.horizontalScrollerVisibility =
            ScrollerVisibility.Hidden;
    }

    private void ClearContainer(VisualElement container)
    {
        if (container == null)
            return;

        if (container is ScrollView scrollView)
            scrollView.contentContainer.Clear();
        else
            container.Clear();
    }

    private void AddToContainer(VisualElement container, VisualElement child)
    {
        if (container == null || child == null)
            return;

        if (container is ScrollView scrollView)
            scrollView.contentContainer.Add(child);
        else
            container.Add(child);
    }

    public bool IsJournalActive()
    {
        return journalContainer != null &&
               journalContainer.style.display == DisplayStyle.Flex;
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

    public void Unlock()
    {
        if (unlocked)
            return;

        unlocked = true;

        Debug.Log("Journal unlocked.");
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
}
