using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestUpdated;

    private QuestState currentMainQuest;

    public QuestState CurrentMainQuest =>
        currentMainQuest;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void StartQuest(
        string title,
        params string[] objectives)
    {
        currentMainQuest =
            new QuestState(title, objectives);

        RaiseUpdated();
    }

    public void CompleteObjective(int index)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >= currentMainQuest.Objectives.Count)
            return;

        currentMainQuest.Objectives[index]
            .Completed = true;

        RaiseUpdated();
    }

    public void SetObjectiveText(
        int index,
        string text)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >= currentMainQuest.Objectives.Count)
            return;

        currentMainQuest.Objectives[index]
            .Text = text;

        RaiseUpdated();
    }

    public void FinishQuest()
    {
        currentMainQuest = null;

        RaiseUpdated();
    }

    private void RaiseUpdated()
    {
        OnQuestUpdated?.Invoke(currentMainQuest);
    }
}