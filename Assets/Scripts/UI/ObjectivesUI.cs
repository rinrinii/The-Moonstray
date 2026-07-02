using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement panel;

    private VisualElement sideQuestContainer;

    private Label titleLabel;
    private Label descriptionLabel;

    public void Initialize(VisualElement root)
    {
        panel =
            root.Q<VisualElement>("ObjectivesPanel");

        sideQuestContainer =
            root.Q<VisualElement>("SideQuestContainer");

        titleLabel =
            root.Q<Label>("MainQuestTitle");

        descriptionLabel =
            root.Q<Label>("MainQuestDescription");

        if (sideQuestContainer != null)
            sideQuestContainer.style.display =
                DisplayStyle.None;

        Hide();

        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated +=
                Refresh;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -=
                Refresh;
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

        StringBuilder builder = new();

        foreach (QuestObjective objective
                 in quest.Objectives)
        {
            builder.Append(
                objective.Completed
                    ? "/ "
                    : "X ");

            builder.AppendLine(objective.Text);
        }

        descriptionLabel.text =
            builder.ToString();
    }

    public void Show()
    {
        panel.style.display =
            DisplayStyle.Flex;
    }

    public void Hide()
    {
        panel.style.display =
            DisplayStyle.None;
    }
}