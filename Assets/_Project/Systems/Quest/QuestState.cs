using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
public class QuestState
{
    public string Title;
    public string Description;
    public string Conditions;
    public string Rewards;
    public QuestData Data;
    public bool Completed;
    public bool IsObjectiveLog;
    public long LastUpdatedOrder;

    public List<QuestObjective> Objectives =
        new();

    public QuestState(string title, params string[] objectives)
    {
        Title = title;

        foreach (string objective in objectives)
            Objectives.Add(new QuestObjective(objective));

        StringBuilder builder = new();

        foreach (string objective in objectives)
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(objective);
        }

        Conditions = builder.ToString();
    }

    public QuestState(QuestData data)
    {
        Data = data;
        Title = data != null ? data.DisplayTitle : string.Empty;
        Description = data != null ? data.description : string.Empty;
        Conditions = data != null ? data.RequiredItemsText : string.Empty;
        Rewards = data != null ? data.RewardText : string.Empty;

        if (!string.IsNullOrWhiteSpace(Conditions))
            Objectives.Add(new QuestObjective(Conditions));
    }
}
