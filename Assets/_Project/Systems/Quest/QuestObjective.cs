using System;

[Serializable]
public class QuestObjective
{
    public string Text;
    public bool Completed;
    public string ObjectiveID;
    public int CurrentAmount;
    public int RequiredAmount = 1;

    public QuestObjective(string text)
    {
        Text = text;
        Completed = false;
    }

    public QuestObjective(QuestObjectiveData data)
    {
        ObjectiveID = data != null ? data.objectiveID : string.Empty;
        RequiredAmount = data != null ? Math.Max(1, data.requiredAmount) : 1;
        CurrentAmount = 0;
        Text = data != null ? data.FormatProgress(0) : string.Empty;
        Completed = false;
    }
}
