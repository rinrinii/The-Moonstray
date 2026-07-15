using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestUpdated;

    private QuestState currentMainQuest;

    private readonly List<QuestState> sideQuests = new();
    private readonly HashSet<QuestData> completedSideQuests = new();

    private QuestData trackedQuestData;

    public QuestState CurrentMainQuest => currentMainQuest;
    public IReadOnlyList<QuestState> SideQuests => sideQuests;
    public QuestData TrackedQuestData => trackedQuestData;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        RefreshSceneQuestMarkers();
    }

    public void StartQuest(
        string title,
        params string[] objectives)
    {
        currentMainQuest =
            new QuestState(title, objectives);

        CurrentObjectiveIndex = 0;

        RaiseUpdated();
    }

    public bool AcceptSideQuest(
        QuestData questData)
    {
        if (questData == null ||
            HasSideQuest(questData) ||
            IsSideQuestCompleted(questData))
        {
            return false;
        }

        QuestState questState =
            new QuestState(questData);

        sideQuests.Add(questState);

        trackedQuestData = questData;

        RefreshSceneQuestMarkers();
        RaiseUpdated(questState);

        return true;
    }

    public bool CanShowSideQuest(
        QuestData questData)
    {
        if (questData == null ||
            IsSideQuestCompleted(questData))
        {
            return false;
        }

        if (GameProgressionManager.Instance == null)
            return true;

        return GameProgressionManager.Instance.IsAtLeast(
            questData.UnlockStage
        );
    }

    public bool HasSideQuest(
        QuestData questData)
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

    public bool IsSideQuestCompleted(
        QuestData questData)
    {
        return questData != null &&
               completedSideQuests.Contains(questData);
    }

    public bool CanSubmitSideQuest(
        QuestData questData,
        out string failureReason)
    {
        failureReason = string.Empty;

        if (questData == null)
        {
            failureReason =
                "No quest selected.";

            return false;
        }

        if (!HasSideQuest(questData))
        {
            failureReason =
                "Accept this quest first.";

            return false;
        }

        if (!HasRequirements(
                questData,
                out failureReason))
        {
            return false;
        }

        return true;
    }

    public bool SubmitSideQuest(
        QuestData questData,
        out string failureReason)
    {
        if (!CanSubmitSideQuest(
                questData,
                out failureReason))
        {
            return false;
        }

        ConsumeRequiredItems(questData);
        GrantRewards(questData);
        CompleteSideQuest(questData);

        failureReason = string.Empty;

        return true;
    }

    public void TrackQuest(
        QuestData questData)
    {
        if (questData == null ||
            !HasSideQuest(questData))
        {
            return;
        }

        trackedQuestData = questData;

        RefreshSceneQuestMarkers();
        RaiseUpdated();
    }

    public bool IsQuestTracked(
        QuestData questData)
    {
        return questData != null &&
               trackedQuestData == questData;
    }

    public void ClearTrackedQuest()
    {
        trackedQuestData = null;

        QuestCompassIndicator.Instance?
            .ClearActiveQuestTarget();

        RaiseUpdated();
    }

    private bool HasRequirements(
        QuestData questData,
        out string failureReason)
    {
        failureReason = string.Empty;

        IReadOnlyList<QuestRequirement> requirements =
            questData.Requirements;

        if (requirements == null ||
            requirements.Count == 0)
        {
            return true;
        }

        List<string> missingRequirements =
            new List<string>();

        foreach (QuestRequirement requirement
                 in requirements)
        {
            if (requirement == null ||
                !requirement.IsValid)
            {
                continue;
            }

            if (requirement.IsItem)
            {
                if (InventorySystem.Instance == null)
                {
                    failureReason =
                        "Inventory is unavailable.";

                    return false;
                }

                int requiredAmount =
                    Mathf.Max(
                        1,
                        requirement.amount
                    );

                int availableAmount =
                    CountInventoryItem(
                        requirement.item
                    );

                if (availableAmount <
                    requiredAmount)
                {
                    missingRequirements.Add(
                        $"Missing {requiredAmount - availableAmount} x {requirement.item.itemName}"
                    );
                }

                continue;
            }

            if (requirement.IsNote)
            {
                if (JournalController.Instance == null)
                {
                    failureReason =
                        "Journal is unavailable.";

                    return false;
                }

                if (!JournalController.Instance.HasNote(
                        requirement.note))
                {
                    missingRequirements.Add(
                        $"Missing journal entry: {requirement.note.title}"
                    );
                }
            }
        }

        if (missingRequirements.Count == 0)
            return true;

        failureReason =
            string.Join(
                "\n",
                missingRequirements
            ) +
            ".";

        return false;
    }

    private int CountInventoryItem(
        ItemData item)
    {
        if (InventorySystem.Instance == null ||
            item == null)
        {
            return 0;
        }

        int count = 0;

        foreach (InventorySystem.Slot slot
                 in InventorySystem.Instance.slots)
        {
            if (IsSameItem(slot.item, item))
                count += slot.amount;
        }

        return count;
    }

    private bool IsSameItem(
        ItemData first,
        ItemData second)
    {
        if (first == null ||
            second == null)
        {
            return false;
        }

        if (first == second)
            return true;

        if (first.itemID != 0 &&
            first.itemID == second.itemID)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(
                   first.itemName
               ) &&
               first.itemName ==
               second.itemName;
    }

    private void ConsumeRequiredItems(
        QuestData questData)
    {
        if (InventorySystem.Instance == null ||
            questData.Requirements == null)
        {
            return;
        }

        foreach (QuestRequirement requirement
                 in questData.Requirements)
        {
            if (requirement == null ||
                !requirement.IsItem)
            {
                continue;
            }

            InventorySystem.Instance.Remove(
                requirement.item,
                Mathf.Max(
                    1,
                    requirement.amount
                )
            );
        }
    }

    private void GrantRewards(
        QuestData questData)
    {
        if (questData.moonCoinReward > 0)
        {
            MoonCoinWallet.Instance?.Add(
                questData.moonCoinReward
            );
        }

        if (InventorySystem.Instance != null &&
            questData.rewardItems != null)
        {
            foreach (QuestItemAmount rewardItem
                     in questData.rewardItems)
            {
                if (rewardItem == null ||
                    rewardItem.item == null)
                {
                    continue;
                }

                InventorySystem.Instance.Add(
                    rewardItem.item,
                    Mathf.Max(
                        1,
                        rewardItem.amount
                    )
                );
            }
        }

        if (JournalController.Instance != null &&
            questData.rewardNotes != null)
        {
            foreach (NoteData note
                     in questData.rewardNotes)
            {
                if (note == null)
                    continue;

                JournalController.Instance.AddNote(
                    note.title,
                    note.content
                );
            }
        }

        if (UpgradeManager.Instance != null &&
            questData.rewardUpgrades != null)
        {
            foreach (UpgradeType upgrade
                     in questData.rewardUpgrades)
            {
                UpgradeManager.Instance
                    .UnlockUpgrade(upgrade);
            }
        }
    }

    private void CompleteSideQuest(
        QuestData questData)
    {
        completedSideQuests.Add(questData);

        for (int i = sideQuests.Count - 1;
             i >= 0;
             i--)
        {
            if (sideQuests[i].Data != questData)
                continue;

            sideQuests[i].Completed = true;
            sideQuests.RemoveAt(i);
        }

        if (trackedQuestData == questData)
        {
            trackedQuestData = null;

            if (sideQuests.Count > 0)
            {
                trackedQuestData =
                    sideQuests[
                        sideQuests.Count - 1
                    ].Data;
            }
        }

        RefreshSceneQuestMarkers();
        RaiseUpdated();
    }

    public void CompleteObjective(
        int index)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >=
            currentMainQuest.Objectives.Count)
        {
            return;
        }

        if (currentMainQuest
            .Objectives[index]
            .Completed)
        {
            return;
        }

        currentMainQuest
            .Objectives[index]
            .Completed = true;

        RaiseUpdated();
    }

    public void CompleteCurrentObjective()
    {
        if (currentMainQuest == null)
            return;

        if (CurrentObjectiveIndex < 0 ||
            CurrentObjectiveIndex >=
            currentMainQuest.Objectives.Count)
        {
            return;
        }

        currentMainQuest
            .Objectives[CurrentObjectiveIndex]
            .Completed = true;

        if (CurrentObjectiveIndex <
            currentMainQuest.Objectives.Count - 1)
        {
            CurrentObjectiveIndex++;
        }

        RaiseUpdated();
    }

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

    public void SetCurrentObjective(
        int index)
    {
        if (currentMainQuest == null)
            return;

        if (index < 0 ||
            index >=
            currentMainQuest.Objectives.Count)
        {
            return;
        }

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
            index >=
            currentMainQuest.Objectives.Count)
        {
            return;
        }

        currentMainQuest
            .Objectives[index]
            .Text = text;

        RaiseUpdated();
    }

    public void FinishQuest()
    {
        currentMainQuest = null;
        CurrentObjectiveIndex = 0;

        RaiseUpdated(currentMainQuest);
    }

    private void RefreshSceneQuestMarkers()
    {
        string currentScene =
            SceneManager
                .GetActiveScene()
                .name;

        MapMarkerTarget[] markers =
            FindObjectsByType<MapMarkerTarget>(
                FindObjectsSortMode.None
            );

        foreach (MapMarkerTarget marker
                 in markers)
        {
            if (marker == null ||
                marker.MarkerType !=
                MapMarkerType.Quest)
            {
                continue;
            }

            QuestData matchingQuest =
                FindActiveQuestForMarker(
                    marker.MarkerID
                );

            bool shouldShow =
                matchingQuest != null &&
                matchingQuest.trackingSceneName ==
                currentScene;

            marker.SetMarkerActive(
                shouldShow
            );
        }

        MapMarkerController.Instance?
            .RefreshMarkers();

        ResolveTrackedQuestCompass(
            currentScene
        );
    }

    private QuestData FindActiveQuestForMarker(
        string markerID)
    {
        if (string.IsNullOrWhiteSpace(markerID))
            return null;

        foreach (QuestState quest
                 in sideQuests)
        {
            if (quest == null ||
                quest.Data == null)
            {
                continue;
            }

            if (quest.Data.trackingMarkerID ==
                markerID)
            {
                return quest.Data;
            }
        }

        return null;
    }

    private void ResolveTrackedQuestCompass(
        string currentScene)
    {
        if (trackedQuestData == null ||
            trackedQuestData.trackingSceneName !=
            currentScene)
        {
            QuestCompassIndicator.Instance?
                .ClearActiveQuestTarget();

            return;
        }

        MapMarkerTarget marker =
            MapMarkerTarget.FindByID(
                trackedQuestData
                    .trackingMarkerID
            );

        if (marker == null ||
            !marker.IsActive)
        {
            QuestCompassIndicator.Instance?
                .ClearActiveQuestTarget();

            return;
        }

        QuestCompassIndicator.Instance?
            .SetActiveQuestTarget(
                marker.transform
            );
    }

    private void RaiseUpdated(
        QuestState updatedQuest = null)
    {
        OnQuestUpdated?.Invoke(
            updatedQuest ??
            currentMainQuest
        );
    }
}