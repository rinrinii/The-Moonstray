using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestBoardController : MonoBehaviour
{
    [Header("Templates")]
    [SerializeField] private VisualTreeAsset questNoteTemplate;

    [Header("Quest Data")]
    [Tooltip("Optional explicit board list. If empty, quests are loaded from Resources/Quests.")]
    [SerializeField] private QuestData[] boardQuests;

    [Header("Settings")]
    [SerializeField] private int notesPerPage = 3;
    [SerializeField] private float noteWidth = 260f;
    [SerializeField] private float noteHeight = 340f;

    private VisualElement questBoardRoot;
    private VisualElement questContainer;
    private VisualElement questDetailsPanel;

    private ScrollView detailDescriptionScroll;
    private Label detailTitle;
    private Label detailDescription;
    private Label detailRequiredItems;
    private Label detailReward;
    private Label detailStatus;

    private Button closeButton;
    private Button detailCloseButton;
    private Button acceptButton;
    private Button prevPageButton;
    private Button nextPageButton;

    private int currentPage = 0;
    private QuestData selectedQuest;
    private readonly List<QuestData> availableQuests = new();
    private readonly PlayerMovementFreezeHandle movementFreeze =
        new();

    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("QuestBoardController: UIDocument missing.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        questBoardRoot = root.Q<VisualElement>("QuestBoardRoot");
        questContainer = root.Q<VisualElement>("QuestContainer");
        questDetailsPanel = root.Q<VisualElement>("QuestDetailsPanel");

        detailDescriptionScroll = root.Q<ScrollView>("DetailDescriptionScroll");
        detailTitle = root.Q<Label>("DetailTitle");
        detailDescription = root.Q<Label>("DetailDescription");
        detailRequiredItems = root.Q<Label>("DetailRequiredItems");
        detailReward = root.Q<Label>("DetailReward");
        detailStatus = root.Q<Label>("DetailStatus");

        closeButton = root.Q<Button>("BoardCloseButton");
        detailCloseButton = root.Q<Button>("DetailCloseButton");
        acceptButton = root.Q<Button>("AcceptButton");
        prevPageButton = root.Q<Button>("PrevPageButton");
        nextPageButton = root.Q<Button>("NextPageButton");

        if (questBoardRoot == null) Debug.LogError("QuestBoardRoot not found.");
        if (questContainer == null) Debug.LogError("QuestContainer not found.");
        if (questDetailsPanel == null) Debug.LogError("QuestDetailsPanel not found.");
        if (detailTitle == null) Debug.LogError("DetailTitle not found.");
        if (detailDescription == null) Debug.LogError("DetailDescription not found.");
        if (detailRequiredItems == null) Debug.LogError("DetailRequiredItems not found.");
        if (detailReward == null) Debug.LogError("DetailReward not found.");
        if (detailStatus == null) Debug.LogWarning("DetailStatus not found.");
        if (closeButton == null) Debug.LogError("CloseButton not found.");
        if (detailCloseButton == null) Debug.LogError("DetailCloseButton not found.");
        if (acceptButton == null) Debug.LogError("AcceptButton not found.");
        if (prevPageButton == null) Debug.LogError("PrevPageButton not found.");
        if (nextPageButton == null) Debug.LogError("NextPageButton not found.");

        if (detailDescriptionScroll != null)
        {
            detailDescriptionScroll.verticalScrollerVisibility =
                ScrollerVisibility.Auto;
            detailDescriptionScroll.horizontalScrollerVisibility =
                ScrollerVisibility.Hidden;
        }

        if (closeButton != null)
            closeButton.clicked += CloseBoard;

        if (detailCloseButton != null)
            detailCloseButton.clicked += CloseQuestDetails;

        if (acceptButton != null)
            acceptButton.clicked += AcceptSelectedQuest;

        if (prevPageButton != null)
            prevPageButton.clicked += PreviousPage;

        if (nextPageButton != null)
            nextPageButton.clicked += NextPage;

        SetupQuestContainer();

        if (questDetailsPanel != null)
            questDetailsPanel.style.display = DisplayStyle.None;

        RefreshAvailableQuests();
        PopulateQuestBoard();
        CloseBoard();
    }

    private void SetupQuestContainer()
    {
        if (questContainer == null)
            return;

        questContainer.style.flexDirection = FlexDirection.Row;
        questContainer.style.justifyContent = Justify.Center;
        questContainer.style.alignItems = Align.Center;
        questContainer.style.flexGrow = 0;
        questContainer.style.flexShrink = 0;
    }

    public void OpenBoard()
    {
        if (questBoardRoot == null)
            return;

        questBoardRoot.style.display = DisplayStyle.Flex;
        movementFreeze.Acquire();
        CloseQuestDetails();
        RefreshAvailableQuests();
        PopulateQuestBoard();
    }

    public void CloseBoard()
    {
        if (questBoardRoot != null)
            questBoardRoot.style.display = DisplayStyle.None;

        CloseQuestDetails();
        movementFreeze.Release();
    }

    private void RefreshAvailableQuests()
    {
        availableQuests.Clear();
        HashSet<QuestData> seenQuests = new();

        if (boardQuests != null && boardQuests.Length > 0)
        {
            foreach (QuestData quest in boardQuests)
                AddAvailableQuest(quest, seenQuests);
        }
        else
        {
            QuestData[] resourceQuests = Resources.LoadAll<QuestData>("Quests");

            System.Array.Sort(resourceQuests, (a, b) =>
                string.Compare(
                    a.DisplayTitle,
                    b.DisplayTitle,
                    System.StringComparison.OrdinalIgnoreCase));

            foreach (QuestData quest in resourceQuests)
                AddAvailableQuest(quest, seenQuests);
        }
    }

    private void AddAvailableQuest(QuestData quest, HashSet<QuestData> seenQuests)
    {
        if (quest == null ||
            quest.category != QuestCategory.Side ||
            seenQuests.Contains(quest))
            return;

        seenQuests.Add(quest);

        if (QuestManager.Instance != null &&
            (!QuestManager.Instance.CanShowSideQuest(quest) ||
             QuestManager.Instance.HasSideQuest(quest)))
        {
            return;
        }

        availableQuests.Add(quest);
    }

    private void PopulateQuestBoard()
    {
        if (questContainer == null || questNoteTemplate == null)
            return;

        questContainer.Clear();

        int totalPages = Mathf.CeilToInt((float)availableQuests.Count / notesPerPage);

        if (totalPages <= 0)
        {
            SetPageButtons(false, false);
            return;
        }

        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        int startIndex = currentPage * notesPerPage;
        int endIndex = Mathf.Min(startIndex + notesPerPage, availableQuests.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            QuestData quest = availableQuests[i];
            TemplateContainer template = questNoteTemplate.Instantiate();

            VisualElement note = template.Q<VisualElement>("QuestNoteRoot");
            Button noteButton = template.Q<Button>("QuestNoteButton");

            note.RemoveFromHierarchy();

            note.style.width = noteWidth;
            note.style.height = noteHeight;
            note.style.marginLeft = 8;
            note.style.marginRight = 8;

            if (noteButton != null)
            {
                noteButton.text = quest.DisplayTitle;

                noteButton.style.width = Length.Percent(100);
                noteButton.style.height = Length.Percent(100);
                noteButton.style.whiteSpace = WhiteSpace.Normal;
                noteButton.style.overflow = Overflow.Hidden;
                noteButton.style.textOverflow = TextOverflow.Ellipsis;

                QuestData capturedQuest = quest;

                noteButton.clicked += () =>
                {
                    ShowQuestDetails(capturedQuest);
                };
            }
            else
            {
                Debug.LogError("QuestNoteButton not found in template.");
            }

            questContainer.Add(note);
        }

        SetPageButtons(currentPage > 0, currentPage < totalPages - 1);
    }

    private void SetPageButtons(bool canGoPrev, bool canGoNext)
    {
        if (prevPageButton != null)
            prevPageButton.SetEnabled(canGoPrev);

        if (nextPageButton != null)
            nextPageButton.SetEnabled(canGoNext);
    }

    private void ShowQuestDetails(QuestData quest)
    {
        selectedQuest = quest;

        if (detailTitle != null)
            detailTitle.text = quest.DisplayTitle;

        if (detailDescription != null)
            detailDescription.text = string.IsNullOrWhiteSpace(quest.description)
                ? "No description listed."
                : quest.description;

        if (detailRequiredItems != null)
            detailRequiredItems.text = quest.RequiredItemsText;

        if (detailReward != null)
            detailReward.text = quest.RewardText;

        RefreshActionButton();

        if (detailStatus != null)
            detailStatus.text = string.Empty;

        if (detailDescriptionScroll != null)
            detailDescriptionScroll.scrollOffset = Vector2.zero;

        if (questDetailsPanel != null)
            questDetailsPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseQuestDetails()
    {
        selectedQuest = null;

        if (questDetailsPanel != null)
            questDetailsPanel.style.display = DisplayStyle.None;
    }

    private void RefreshActionButton()
    {
        if (acceptButton == null || selectedQuest == null)
            return;

        bool accepted = QuestManager.Instance != null &&
            QuestManager.Instance.HasSideQuest(selectedQuest);

        acceptButton.text = accepted
            ? "Accepted"
            : "Accept Quest";

        acceptButton.SetEnabled(!accepted);
    }

    private void AcceptSelectedQuest()
    {
        if (selectedQuest == null)
            return;

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestBoardController could not find QuestManager.");
            return;
        }

        if (QuestManager.Instance.AcceptSideQuest(selectedQuest))
        {
            Debug.Log("Accepted quest: " + selectedQuest.DisplayTitle);

            if (detailStatus != null)
                detailStatus.text = "Quest accepted.";
        }

        RefreshAvailableQuests();
        CloseQuestDetails();
        PopulateQuestBoard();
    }

    private void NextPage()
    {
        currentPage++;
        PopulateQuestBoard();
    }

    private void PreviousPage()
    {
        currentPage--;
        PopulateQuestBoard();
    }
}
