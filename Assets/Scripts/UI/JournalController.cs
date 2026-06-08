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

    private void Start()
    {
        var ui = GameplayUIManager.Instance;
        journalContainer = ui.JournalContainer;

        if (journalContainer != null)
        {
            InitializeTemplateBindings(journalContainer);
        }
    }

    private void InitializeTemplateBindings(VisualElement root)
    {
        mainQuestListContainer = root.Q<VisualElement>("MainQuestList-Container");
        sideQuestListContainer = root.Q<VisualElement>("SideQuestList-Container");
        questTitleLabel = root.Q<Label>("QuestTitle");
        questDetailsLabel = root.Q<Label>("QuestDetails");
        questConditionLabel = root.Q<Label>("QuestCondition");
        
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
        GameplayUIManager.Instance.SuppressSecondaryPanels(); // Ensure the map or other full-screen overlays shut down safely
        isJournalOpen = true;
        journalContainer.style.display = DisplayStyle.Flex;
        
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

        // Concrete implementation snippet demonstrating how to pass runtime collections safely into your custom USS styles
        for (int i = 1; i <= 3; i++)
        {
            Button runtimeQuestButton = new Button();
            runtimeQuestButton.text = $"Blight Investigation Task #{i}";
            
            // Inject the identical style class listed inside your text-editor stylesheets
            runtimeQuestButton.AddToClassList("journalButton");

            // Strip out default layout overrides
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