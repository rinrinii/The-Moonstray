using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestUpdated;

    private QuestState currentMainQuest;
    private readonly List<QuestState> sideQuests = new();
    private readonly HashSet<QuestData> completedSideQuests = new();

    public QuestState CurrentMainQuest => currentMainQuest;
    public IReadOnlyList<QuestState> SideQuests => sideQuests;

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

    public bool AcceptSideQuest(QuestData questData)
    {
        if (questData == null ||
            HasSideQuest(questData) ||
            IsSideQuestCompleted(questData))
        {
            return false;
        }

        QuestState questState = new(questData);
        sideQuests.Add(questState);
        RaiseUpdated();

        return true;
    }

    public bool CanShowSideQuest(QuestData questData)
    {
        if (questData == null || IsSideQuestCompleted(questData))
            return false;

        if (GameProgressionManager.Instance == null)
            return true;

        return GameProgressionManager.Instance.IsAtLeast(
            questData.UnlockStage);
    }

    public bool HasSideQuest(QuestData questData)
    {
        if (questData == null)
            return false;

        foreach (QuestState sideQuest in sideQuests)
        {
            if (sideQuest.Data == questData)
                return true;
        }

        return false;
    }

    public bool IsSideQuestCompleted(QuestData questData)
    {
        return questData != null &&
            completedSideQuests.Contains(questData);
    }

    public bool CanSubmitSideQuest(QuestData questData, out string failureReason)
    {
        failureReason = string.Empty;

        if (questData == null)
        {
            failureReason = "No quest selected.";
            return false;
        }

        if (!HasSideQuest(questData))
        {
            failureReason = "Accept this quest first.";
            return false;
        }

        if (!HasRequirements(questData, out failureReason))
            return false;

        return true;
    }

    public bool SubmitSideQuest(QuestData questData, out string failureReason)
    {
        if (!CanSubmitSideQuest(questData, out failureReason))
            return false;

        ConsumeRequiredItems(questData);
        GrantRewards(questData);
        CompleteSideQuest(questData);
        failureReason = string.Empty;

        return true;
    }

    private bool HasRequirements(QuestData questData, out string failureReason)
    {
        failureReason = string.Empty;

        IReadOnlyList<QuestRequirement> requirements =
            questData.Requirements;

        if (requirements == null || requirements.Count == 0)
        {
            return true;
        }

        List<string> missingRequirements = new();

        foreach (QuestRequirement requirement in requirements)
        {
            if (requirement == null || !requirement.IsValid)
                continue;

            if (requirement.IsItem)
            {
                if (InventorySystem.Instance == null)
                {
                    failureReason = "Inventory is unavailable.";
                    return false;
                }

                int requiredAmount = Mathf.Max(1, requirement.amount);
                int availableAmount = CountInventoryItem(requirement.item);

                if (availableAmount < requiredAmount)
                {
                    missingRequirements.Add(
                        $"Missing {requiredAmount - availableAmount} x {requirement.item.itemName}");
                }

                continue;
            }

            if (requirement.IsNote)
            {
                if (JournalController.Instance == null)
                {
                    failureReason = "Journal is unavailable.";
                    return false;
                }

                if (!JournalController.Instance.HasNote(requirement.note))
                {
                    missingRequirements.Add(
                        $"Missing journal entry: {requirement.note.title}");
                }
            }
        }

        if (missingRequirements.Count == 0)
            return true;

        failureReason = string.Join("\n", missingRequirements) + ".";
        return false;
    }

    private int CountInventoryItem(ItemData item)
    {
        if (InventorySystem.Instance == null || item == null)
            return 0;

        int count = 0;

        foreach (InventorySystem.Slot slot in InventorySystem.Instance.slots)
        {
            if (IsSameItem(slot.item, item))
                count += slot.amount;
        }

        return count;
    }

    private bool IsSameItem(ItemData first, ItemData second)
    {
        if (first == null || second == null)
            return false;

        if (first == second)
            return true;

        if (first.itemID != 0 && first.itemID == second.itemID)
            return true;

        return !string.IsNullOrWhiteSpace(first.itemName) &&
            first.itemName == second.itemName;
    }

    private void ConsumeRequiredItems(QuestData questData)
    {
        if (InventorySystem.Instance == null ||
            questData.Requirements == null)
        {
            return;
        }

        foreach (QuestRequirement requirement in questData.Requirements)
        {
            if (requirement == null || !requirement.IsItem)
                continue;

            InventorySystem.Instance.Remove(
                requirement.item,
                Mathf.Max(1, requirement.amount));
        }
    }

    private void GrantRewards(QuestData questData)
    {
        if (questData.moonCoinReward > 0)
            MoonCoinWallet.Instance?.Add(questData.moonCoinReward);

        if (InventorySystem.Instance != null &&
            questData.rewardItems != null)
        {
            foreach (QuestItemAmount rewardItem in questData.rewardItems)
            {
                if (rewardItem == null || rewardItem.item == null)
                    continue;

                InventorySystem.Instance.Add(
                    rewardItem.item,
                    Mathf.Max(1, rewardItem.amount));
            }
        }

        if (JournalController.Instance != null &&
            questData.rewardNotes != null)
        {
            foreach (NoteData note in questData.rewardNotes)
            {
                if (note == null)
                    continue;

                JournalController.Instance.AddNote(note.title, note.content);
            }
        }

        if (UpgradeManager.Instance != null &&
            questData.rewardUpgrades != null)
        {
            foreach (UpgradeType upgrade in questData.rewardUpgrades)
                UpgradeManager.Instance.UnlockUpgrade(upgrade);
        }
    }

    private void CompleteSideQuest(QuestData questData)
    {
        completedSideQuests.Add(questData);

        for (int i = sideQuests.Count - 1; i >= 0; i--)
        {
            if (sideQuests[i].Data == questData)
            {
                sideQuests[i].Completed = true;
                sideQuests.RemoveAt(i);
            }
        }

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

        RaiseUpdated(currentMainQuest);
    }

    private void RaiseUpdated(QuestState updatedQuest = null)
    {
        OnQuestUpdated?.Invoke(updatedQuest ?? currentMainQuest);
    }
}
