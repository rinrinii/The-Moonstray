using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement panel;
    private VisualElement hudGroup;

    private Label titleLabel;
    private Label descriptionLabel;
    private VisualElement trackingHint;
    private VisualElement trackingKeyIcon;
    private Label trackingHintLabel;

    public QuestObjectiveData CurrentObjectiveData { get; private set; }

    public static ObjectivesUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize(VisualElement root)
    {
        panel = root.Q<VisualElement>("ObjectivesPanel");
        hudGroup = root.Q<VisualElement>("ObjectivesHUDGroup");

        titleLabel =
            root.Q<Label>("MainQuestTitle");

        descriptionLabel =
            root.Q<Label>("MainQuestDescription");

        trackingHint = root.Q<VisualElement>("ObjectiveTrackingHint");
        trackingKeyIcon = root.Q<VisualElement>("ObjectiveTrackingKeyIcon");
        trackingHintLabel = root.Q<Label>("ObjectiveTrackingHintLabel");

        Hide();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += Refresh;
            Refresh(QuestManager.Instance.GetDisplayedQuest());
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= Refresh;
    }

    private void Update()
    {
        RefreshTrackingHint();
    }

    private void RefreshTrackingHint()
    {
        if (trackingHint == null)
            return;

        QuestObjectiveData objective = CurrentObjectiveData;

        if (objective == null)
        {
            trackingHint.style.display = DisplayStyle.None;
            return;
        }

        if (objective.trackingMode == ObjectiveTrackingMode.None)
        {
            bool isGeneralCollection =
                objective.type == QuestObjectiveType.Collect;

            trackingHint.style.display = isGeneralCollection
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (isGeneralCollection)
            {
                trackingKeyIcon.style.display = DisplayStyle.None;
                bool isInPossibleArea =
                    objective.possibleScenes != null &&
                    objective.possibleScenes.Contains(
                        SceneManager.GetActiveScene().name);

                trackingHintLabel.text = isInPossibleArea
                    ? "Currently in tracked location"
                    : "Check journal for possible areas";
            }

            return;
        }

        trackingHint.style.display = DisplayStyle.Flex;

        bool isInsideSearchArea =
            objective.trackingMode == ObjectiveTrackingMode.SearchArea &&
            QuestCompassIndicator.Instance != null &&
            QuestCompassIndicator.Instance.IsInsideTrackedArea;

        trackingKeyIcon.style.display = isInsideSearchArea
            ? DisplayStyle.None
            : DisplayStyle.Flex;

        trackingHintLabel.text = isInsideSearchArea
            ? "Currently in tracked location"
            : "Track current objective";
    }

    private void Refresh(QuestState quest)
    {
        quest = QuestManager.Instance?.GetDisplayedQuest() ?? quest;

        if (quest == null)
        {
            Hide();
            return;
        }

        Show();

        titleLabel.text = quest.Title;

        if (quest.IsObjectiveLog)
        {
            descriptionLabel.text = quest.CurrentObjectiveText;
            CurrentObjectiveData = FindCurrentObjectiveData(quest);
            return;
        }

        CurrentObjectiveData = null;

        StringBuilder builder = new();

        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            builder.AppendLine(quest.Objectives[i].Text);
        }

        descriptionLabel.text = builder.ToString();
    }

    public void Show()
    {
        if (panel == null)
            return;

        hudGroup.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (panel == null)
            return;

        titleLabel.text = string.Empty;
        descriptionLabel.text = string.Empty;
        CurrentObjectiveData = null;

        if (trackingHint != null)
            trackingHint.style.display = DisplayStyle.None;

        hudGroup.style.display = DisplayStyle.None;
    }

    public void Clear()
    {
        Hide();
    }

    public void RefreshDisplayedQuest()
    {
        Refresh(QuestManager.Instance?.GetDisplayedQuest());
    }

    public void SetObjective(string title, string description)
    {
        if (panel == null)
            return;

        QuestState quest = QuestManager.Instance?.RecordObjectiveForJournal(
            title,
            description);

        Refresh(QuestManager.Instance?.GetDisplayedQuest() ?? quest);
    }

    public void SetObjective(
        string questID,
        string objectiveID,
        int currentAmount)
    {
        QuestState quest = QuestManager.Instance?.ActivateObjective(
            questID,
            objectiveID,
            currentAmount);

        if (quest == null)
            return;

        Refresh(QuestManager.Instance?.GetDisplayedQuest() ?? quest);
    }

    private static QuestObjectiveData FindCurrentObjectiveData(QuestState quest)
    {
        if (quest?.Data?.objectives == null)
            return null;

        foreach (QuestObjectiveData objective in quest.Data.objectives)
        {
            if (objective != null &&
                objective.objectiveID == quest.CurrentObjectiveID)
            {
                return objective;
            }
        }

        return null;
    }
}
