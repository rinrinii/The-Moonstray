using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement panel;
    private VisualElement hudGroup;

    private Label titleLabel;
    private Label descriptionLabel;

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

        Hide();

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += Refresh;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= Refresh;
    }

    private void Refresh(QuestState quest)
    {
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

        hudGroup.style.display = DisplayStyle.None;
    }

    public void Clear()
    {
        Hide();
    }

    public void SetObjective(string title, string description)
    {
        if (panel == null)
            return;

        Show();

        titleLabel.text = title;
        descriptionLabel.text = description;
        CurrentObjectiveData = null;

        QuestManager.Instance?.RecordObjectiveForJournal(
            title,
            description);
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

        Show();
        titleLabel.text = quest.Title;
        descriptionLabel.text = quest.CurrentObjectiveText;
        CurrentObjectiveData = FindCurrentObjectiveData(quest);
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
