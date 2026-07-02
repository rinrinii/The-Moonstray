using System;

[Serializable]
public class QuestObjective
{
    public string Text;
    public bool Completed;

    public QuestObjective(string text)
    {
        Text = text;
        Completed = false;
    }
}