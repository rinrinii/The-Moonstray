using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestBoardController : MonoBehaviour
{
    [Header("Templates")]
    [SerializeField] private VisualTreeAsset questNoteTemplate;

    [Header("Settings")]
    [SerializeField] private int notesPerPage = 3;
    [SerializeField] private float noteWidth = 180f;
    [SerializeField] private float noteHeight = 220f;

    private VisualElement questBoardRoot;
    private VisualElement questContainer;
    private VisualElement questDetailsPanel;

    private Label detailTitle;
    private Label detailDescription;
    private Label detailReward;

    private Button closeButton;
    private Button detailCloseButton;
    private Button acceptButton;
    private Button prevPageButton;
    private Button nextPageButton;

    private int currentPage = 0;
    private MockQuest selectedQuest;

    private readonly List<MockQuest> mockQuests = new()
    {
        new MockQuest("Gather Moonflowers", "Collect 5 moonflowers near Springtide Meadows.", "10 Moon Coins"),
        new MockQuest("Lost Satchel", "Find the missing satchel near the village path.", "Healing Herb"),
        new MockQuest("Repair the Lantern", "Gather materials to repair the old village lantern.", "Lantern Charm"),
        new MockQuest("Winter Tracks", "Investigate strange tracks in the Pale Snowfields.", "Wolf Fragment"),
        new MockQuest("Herbal Help", "Bring herbs to the village healer.", "Potion Bundle")
    };

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

        detailTitle = root.Q<Label>("DetailTitle");
        detailDescription = root.Q<Label>("DetailDescription");
        detailReward = root.Q<Label>("DetailReward");

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
        if (detailReward == null) Debug.LogError("DetailReward not found.");
        if (closeButton == null) Debug.LogError("CloseButton not found.");
        if (detailCloseButton == null) Debug.LogError("DetailCloseButton not found.");
        if (acceptButton == null) Debug.LogError("AcceptButton not found.");
        if (prevPageButton == null) Debug.LogError("PrevPageButton not found.");
        if (nextPageButton == null) Debug.LogError("NextPageButton not found.");

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
        CloseQuestDetails();
        PopulateQuestBoard();
    }

    public void CloseBoard()
    {
        if (questBoardRoot != null)
            questBoardRoot.style.display = DisplayStyle.None;

        CloseQuestDetails();
    }

    private void PopulateQuestBoard()
    {
        if (questContainer == null || questNoteTemplate == null)
            return;

        questContainer.Clear();

        int totalPages = Mathf.CeilToInt((float)mockQuests.Count / notesPerPage);

        if (totalPages <= 0)
        {
            SetPageButtons(false, false);
            return;
        }

        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        int startIndex = currentPage * notesPerPage;
        int endIndex = Mathf.Min(startIndex + notesPerPage, mockQuests.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            MockQuest quest = mockQuests[i];
            TemplateContainer template = questNoteTemplate.Instantiate();

            VisualElement note = template.Q<VisualElement>("QuestNoteRoot");
            Button noteButton = template.Q<Button>("QuestNoteButton");

            note.RemoveFromHierarchy();

            note.style.marginLeft = 8;
            note.style.marginRight = 8;


            if (noteButton != null)
            {
                noteButton.text = quest.Title;

                noteButton.style.width = Length.Percent(100);
                noteButton.style.height = Length.Percent(100);
                noteButton.style.whiteSpace = WhiteSpace.Normal;
                noteButton.style.overflow = Overflow.Hidden;
                noteButton.style.textOverflow = TextOverflow.Ellipsis;

                MockQuest capturedQuest = quest;

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

    private void ShowQuestDetails(MockQuest quest)
    {
        selectedQuest = quest;

        if (detailTitle != null)
            detailTitle.text = quest.Title;

        if (detailDescription != null)
            detailDescription.text = quest.Description;

        if (detailReward != null)
            detailReward.text = "Reward: " + quest.Reward;

        if (questDetailsPanel != null)
            questDetailsPanel.style.display = DisplayStyle.Flex;
    }

    private void CloseQuestDetails()
    {
        selectedQuest = null;

        if (questDetailsPanel != null)
            questDetailsPanel.style.display = DisplayStyle.None;
    }

    private void AcceptSelectedQuest()
    {
        if (selectedQuest == null)
            return;

        Debug.Log("Accepted quest: " + selectedQuest.Title);

        mockQuests.Remove(selectedQuest);

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

    private class MockQuest
    {
        public string Title;
        public string Description;
        public string Reward;

        public MockQuest(string title, string description, string reward)
        {
            Title = title;
            Description = description;
            Reward = reward;
        }
    }
}