using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestUpdated;

    private QuestState currentMainQuest;

    public QuestState CurrentMainQuest => currentMainQuest;

    /// <summary>
    /// Which objective is currently active.
    /// </summary>
    public int CurrentObjectiveIndex { get; private set; }

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
        currentMainQuest = new QuestState(title, objectives);

        CurrentObjectiveIndex = 0;

        RaiseUpdated();
    }

    /// <summary>
    /// Completes a specific objective.
    /// (Useful if objectives can be completed out of order.)
    /// </summary>
    public void CompleteObjective(int index)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >= currentMainQuest.Objectives.Count)
            return;

        if (currentMainQuest.Objectives[index].Completed)
            return;

        currentMainQuest.Objectives[index].Completed = true;

        RaiseUpdated();
    }

    /// <summary>
    /// Completes the current objective and advances to the next.
    /// </summary>
    public void CompleteCurrentObjective()
    {
        if (currentMainQuest == null)
            return;

        if (CurrentObjectiveIndex < 0 ||
            CurrentObjectiveIndex >= currentMainQuest.Objectives.Count)
            return;

        currentMainQuest.Objectives[CurrentObjectiveIndex]
            .Completed = true;

        if (CurrentObjectiveIndex <
            currentMainQuest.Objectives.Count - 1)
        {
            CurrentObjectiveIndex++;
        }

        RaiseUpdated();
    }

    /// <summary>
    /// Advances the active objective without marking it complete.
    /// </summary>
    public void AdvanceObjective()
    {
        if (currentMainQuest == null)
            return;

        if (CurrentObjectiveIndex <
            currentMainQuest.Objectives.Count - 1)
        {
            CurrentObjectiveIndex++;
            RaiseUpdated();
        }
    }

    /// <summary>
    /// Sets the active objective.
    /// </summary>
    public void SetCurrentObjective(int index)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >= currentMainQuest.Objectives.Count)
            return;

        CurrentObjectiveIndex = index;

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

        currentMainQuest.Objectives[index].Text = text;

        RaiseUpdated();
    }

    public void FinishQuest()
    {
        currentMainQuest = null;
        CurrentObjectiveIndex = 0;

        RaiseUpdated();
    }

    private void RaiseUpdated()
    {
        OnQuestUpdated?.Invoke(currentMainQuest);
    }
}