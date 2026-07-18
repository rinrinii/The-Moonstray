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
    public string CurrentObjectiveID;

    public List<QuestObjective> Objectives =
        new();

    public string CurrentObjectiveText
    {
        get
        {
            foreach (QuestObjective objective in Objectives)
            {
                if (objective.ObjectiveID == CurrentObjectiveID)
                    return objective.Text;
            }

            return Objectives.Count > 0
                ? Objectives[Objectives.Count - 1].Text
                : string.Empty;
        }
    }

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

        if (data != null && data.objectives != null && data.objectives.Count > 0)
        {
            IsObjectiveLog = true;

            foreach (QuestObjectiveData objective in data.objectives)
                Objectives.Add(new QuestObjective(objective));

            QuestObjectiveData firstObjective = data.objectives[0];

            if (firstObjective != null)
            {
                CurrentObjectiveID = firstObjective.objectiveID;
                Conditions = $"Current: {firstObjective.description}";

                if (!string.IsNullOrWhiteSpace(firstObjective.PossibleAreasText))
                    Conditions += $"\n{firstObjective.PossibleAreasText}";
            }
        }
        else if (!string.IsNullOrWhiteSpace(Conditions))
            Objectives.Add(new QuestObjective(Conditions));
    }
}
