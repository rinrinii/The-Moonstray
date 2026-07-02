using System;
using System.Collections.Generic;

[Serializable]
public class QuestState
{
    public string Title;

    public List<QuestObjective> Objectives =
        new();

    public QuestState(string title, params string[] objectives)
    {
        Title = title;

        foreach (string objective in objectives)
            Objectives.Add(new QuestObjective(objective));
    }
}