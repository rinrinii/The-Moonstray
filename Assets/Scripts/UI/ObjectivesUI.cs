using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement panel;

    private VisualElement sideQuestContainer;

    private Label titleLabel;
    private Label descriptionLabel;

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

        sideQuestContainer =
            root.Q<VisualElement>("SideQuestContainer");

        titleLabel =
            root.Q<Label>("MainQuestTitle");

        descriptionLabel =
            root.Q<Label>("MainQuestDescription");

        if (sideQuestContainer != null)
            sideQuestContainer.style.display = DisplayStyle.None;

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
            descriptionLabel.text = quest.Description;
            return;
        }

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

        panel.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (panel == null)
            return;

        titleLabel.text = string.Empty;
        descriptionLabel.text = string.Empty;

        panel.style.display = DisplayStyle.None;
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

        QuestManager.Instance?.RecordObjectiveForJournal(
            title,
            description);
    }
}
