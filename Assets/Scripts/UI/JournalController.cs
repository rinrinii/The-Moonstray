using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class JournalController : MonoBehaviour
{
    private VisualElement journalContainer;
    private bool isJournalOpen = false;

    private VisualElement mainQuestListContainer;
    private VisualElement sideQuestListContainer;
    private Label questTitleLabel;
    private Label questDetailsLabel;
    private Label questConditionLabel;
    private Button closeButton;

    private void Start()
    {
        var ui = GameplayUIManager.Instance;
        journalContainer = ui.JournalContainer;

        if (journalContainer != null)
        {
            InitializeTemplateBindings(journalContainer);
            CloseJournal();
        }
    }

    private void InitializeTemplateBindings(VisualElement root)
    {
        mainQuestListContainer = root.Q<VisualElement>("MainQuestList-Container");
        sideQuestListContainer = root.Q<VisualElement>("SideQuestList-Container");
        questTitleLabel = root.Q<Label>("QuestTitle");
        questDetailsLabel = root.Q<Label>("QuestDetails");
        questConditionLabel = root.Q<Label>("QuestCondition");
        closeButton = root.Q<Button>("CloseButton");

        if (closeButton != null)
        {
            closeButton.pickingMode = PickingMode.Position;
            closeButton.clicked += CloseJournal;
        }

        ClearMockElements();
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

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (isJournalOpen) CloseJournal();
            else OpenJournal();
        }
    }

    public void OpenJournal()
    {
        if (journalContainer == null) return;

        GameplayUIManager.Instance.SuppressSecondaryPanels();

        isJournalOpen = true;
        journalContainer.style.display = DisplayStyle.Flex;
        journalContainer.pickingMode = PickingMode.Position;

        RenderActiveJournalData();
    }

    public void CloseJournal()
    {
        if (journalContainer == null) return;

        isJournalOpen = false;
        journalContainer.style.display = DisplayStyle.None;
    }

    private void RenderActiveJournalData()
    {
        ClearMockElements();

        for (int i = 1; i <= 3; i++)
        {
            Button runtimeQuestButton = new Button();
            runtimeQuestButton.text = $"Blight Investigation Task #{i}";
            runtimeQuestButton.AddToClassList("journalButton");

            runtimeQuestButton.style.borderTopWidth = 0;
            runtimeQuestButton.style.borderBottomWidth = 0;
            runtimeQuestButton.style.borderLeftWidth = 0;
            runtimeQuestButton.style.borderRightWidth = 0;

            string abstractSummary = $"Details for step {i}: Cleanse the remaining hazard metrics scattered across the sandbox quadrants.";
            string goalCondition = $"Status: 0 / {i} Cleansed";

            runtimeQuestButton.clicked += () => PopulateFocusedQuestView(runtimeQuestButton.text, abstractSummary, goalCondition);

            mainQuestListContainer?.Add(runtimeQuestButton);
        }
    }

    private void PopulateFocusedQuestView(string title, string details, string condition)
    {
        if (questTitleLabel != null) questTitleLabel.text = title;
        if (questDetailsLabel != null) questDetailsLabel.text = details;
        if (questConditionLabel != null) questConditionLabel.text = condition;
    }

    public bool IsJournalActive() => isJournalOpen;
}