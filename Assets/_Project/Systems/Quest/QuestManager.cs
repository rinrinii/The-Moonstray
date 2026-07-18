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
    private readonly List<QuestState> objectiveJournalQuests = new();
    private readonly Dictionary<string, QuestData> questDataByID = new();

    private QuestData trackedQuestData;
    private QuestState trackedQuestState;
    private QuestState currentObjectiveJournalQuest;
    private long questUpdateOrder;
    private InventorySystem subscribedInventory;

    public QuestState CurrentMainQuest => currentMainQuest;
    public IReadOnlyList<QuestState> SideQuests => sideQuests;
    public IReadOnlyList<QuestState> ObjectiveJournalQuests =>
        objectiveJournalQuests;
    public QuestData TrackedQuestData => trackedQuestData;
    public QuestState TrackedQuestState => trackedQuestState;

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
        EnsureInventorySubscription();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveInventorySubscription();
    }

    private void Start()
    {
        EnsureInventorySubscription();
        RefreshSideQuestRequirementStates();
    }

    private void Update()
    {
        EnsureInventorySubscription();
        RefreshSideQuestRequirementStates();
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        EnsureInventorySubscription();
        RefreshSceneQuestMarkers();
        RefreshSideQuestRequirementStates();
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

    public QuestState RecordObjectiveForJournal(
        string title,
        string description)
    {
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        QuestState quest = null;

        foreach (QuestState candidate in objectiveJournalQuests)
        {
            if (candidate.Title == title)
            {
                quest = candidate;
                break;
            }
        }

        if (quest == null)
        {
            quest = new QuestState(title)
            {
                IsObjectiveLog = true
            };

            objectiveJournalQuests.Add(quest);
        }

        if (currentObjectiveJournalQuest != null &&
            currentObjectiveJournalQuest != quest)
        {
            CompleteLatestObjective(currentObjectiveJournalQuest);
            currentObjectiveJournalQuest.Completed = true;
            RebuildObjectiveLogText(currentObjectiveJournalQuest);
        }

        UpdateObjectiveLogStep(quest, description);

        quest.Completed = false;
        currentObjectiveJournalQuest = quest;

        RebuildObjectiveLogText(quest);
        RaiseUpdated(quest);

        return quest;
    }

    public QuestState ActivateObjective(
        string questID,
        string objectiveID,
        int currentAmount = 0)
    {
        QuestData data = FindQuestData(questID);

        if (data == null)
        {
            Debug.LogWarning($"QuestData not found for ID '{questID}'.");
            return null;
        }

        QuestObjectiveData objectiveData = null;

        foreach (QuestObjectiveData candidate in data.objectives)
        {
            if (candidate != null && candidate.objectiveID == objectiveID)
            {
                objectiveData = candidate;
                break;
            }
        }

        if (objectiveData == null)
        {
            Debug.LogWarning(
                $"Objective '{objectiveID}' not found in quest '{questID}'.");
            return null;
        }

        QuestState quest = null;

        foreach (QuestState candidate in objectiveJournalQuests)
        {
            if (candidate.Data == data)
            {
                quest = candidate;
                break;
            }
        }

        if (quest == null)
        {
            quest = new QuestState(data);
            objectiveJournalQuests.Add(quest);
        }

        if (currentObjectiveJournalQuest != null &&
            currentObjectiveJournalQuest != quest)
        {
            CompleteActiveAssetObjective(currentObjectiveJournalQuest);
            currentObjectiveJournalQuest.Completed = true;
            RebuildAssetObjectiveHistory(currentObjectiveJournalQuest);
        }

        if (!string.IsNullOrWhiteSpace(quest.CurrentObjectiveID) &&
            quest.CurrentObjectiveID != objectiveID)
        {
            CompleteActiveAssetObjective(quest);
        }

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID != objectiveID)
                continue;

            objective.CurrentAmount = Mathf.Max(0, currentAmount);
            objective.RequiredAmount = Mathf.Max(1, objectiveData.requiredAmount);
            objective.Text = objectiveData.FormatProgress(currentAmount);
            objective.Completed = false;
            break;
        }

        quest.CurrentObjectiveID = objectiveID;
        quest.Completed = false;
        currentObjectiveJournalQuest = quest;

        RebuildAssetObjectiveHistory(quest);
        RaiseUpdated(quest);

        return quest;
    }

    private QuestData FindQuestData(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID))
            return null;

        if (questDataByID.Count == 0)
            BuildQuestDataLookup();

        questDataByID.TryGetValue(questID, out QuestData questData);
        return questData;
    }

    private void BuildQuestDataLookup()
    {
        QuestData[] quests = Resources.LoadAll<QuestData>("Quests");

        foreach (QuestData quest in quests)
        {
            if (quest == null || string.IsNullOrWhiteSpace(quest.questID))
                continue;

            if (questDataByID.ContainsKey(quest.questID))
            {
                Debug.LogWarning($"Duplicate quest ID '{quest.questID}'.");
                continue;
            }

            questDataByID.Add(quest.questID, quest);
        }
    }

    private static void CompleteActiveAssetObjective(QuestState quest)
    {
        if (quest == null)
            return;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID == quest.CurrentObjectiveID)
            {
                objective.Completed = true;
                return;
            }
        }
    }

    private static void RebuildAssetObjectiveHistory(QuestState quest)
    {
        if (quest == null || quest.Data == null)
            return;

        System.Text.StringBuilder history = new();

        foreach (QuestObjective objective in quest.Objectives)
        {
            bool isCurrent = objective.ObjectiveID == quest.CurrentObjectiveID;

            if (!objective.Completed && !isCurrent)
                continue;

            if (history.Length > 0)
                history.AppendLine();

            history.Append(objective.Completed ? "Completed: " : "Current: ");
            history.Append(objective.Text);

            if (isCurrent)
            {
                QuestObjectiveData objectiveData = null;

                foreach (QuestObjectiveData candidate in quest.Data.objectives)
                {
                    if (candidate != null &&
                        candidate.objectiveID == objective.ObjectiveID)
                    {
                        objectiveData = candidate;
                        break;
                    }
                }

                if (objectiveData != null &&
                    !string.IsNullOrWhiteSpace(objectiveData.PossibleAreasText))
                {
                    history.AppendLine();
                    history.Append(objectiveData.PossibleAreasText);
                }
            }
        }

        quest.Conditions = history.ToString();
    }

    private static void UpdateObjectiveLogStep(
        QuestState quest,
        string description)
    {
        if (quest.Objectives.Count == 0)
        {
            quest.Objectives.Add(new QuestObjective(description));
            return;
        }

        QuestObjective latest =
            quest.Objectives[quest.Objectives.Count - 1];

        if (GetObjectiveStepKey(latest.Text) ==
            GetObjectiveStepKey(description))
        {
            latest.Text = description;
            latest.Completed = false;
            return;
        }

        latest.Completed = true;
        quest.Objectives.Add(new QuestObjective(description));
    }

    private static string GetObjectiveStepKey(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        int counterStart = text.LastIndexOf(" (");

        if (counterStart >= 0 &&
            text.EndsWith(")") &&
            text.IndexOf('/', counterStart) >= 0)
        {
            return text.Substring(0, counterStart);
        }

        return text;
    }

    private static void CompleteLatestObjective(QuestState quest)
    {
        if (quest.Objectives.Count == 0)
            return;

        quest.Objectives[quest.Objectives.Count - 1].Completed = true;
    }

    private static void RebuildObjectiveLogText(QuestState quest)
    {
        if (quest.Objectives.Count == 0)
            return;

        QuestObjective latest =
            quest.Objectives[quest.Objectives.Count - 1];

        quest.Description = latest.Text;

        System.Text.StringBuilder history = new();

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (history.Length > 0)
                history.AppendLine();

            history.Append(objective.Completed ? "Completed: " : "Current: ");
            history.Append(objective.Text);
        }

        quest.Conditions = history.ToString();
    }

    public bool AcceptSideQuest(
        QuestData questData)
    {
        if (questData == null ||
            questData.category != QuestCategory.Side ||
            HasSideQuest(questData) ||
            IsSideQuestCompleted(questData))
        {
            return false;
        }

        QuestState questState =
            new QuestState(questData);

        sideQuests.Add(questState);

        RefreshSideQuestRequirementState(questState);

        RefreshSceneQuestMarkers();
        RaiseUpdated(questState);

        return true;
    }

    private void EnsureInventorySubscription()
    {
        InventorySystem inventory = InventorySystem.Instance;

        if (inventory == subscribedInventory)
            return;

        RemoveInventorySubscription();
        subscribedInventory = inventory;

        if (subscribedInventory != null)
            subscribedInventory.OnInventoryChanged +=
                HandleInventoryChanged;
    }

    private void RemoveInventorySubscription()
    {
        if (subscribedInventory != null)
        {
            subscribedInventory.OnInventoryChanged -=
                HandleInventoryChanged;
        }

        subscribedInventory = null;
    }

    private void HandleInventoryChanged()
    {
        RefreshSideQuestRequirementStates();
    }

    private void RefreshSideQuestRequirementStates()
    {
        foreach (QuestState sideQuest in sideQuests)
            RefreshSideQuestRequirementState(sideQuest);
    }

    private void RefreshSideQuestRequirementState(QuestState quest)
    {
        if (quest?.Data?.objectives == null ||
            quest.Data.objectives.Count < 2)
        {
            return;
        }

        QuestObjectiveData submitObjective = null;
        QuestObjectiveData collectionObjective = null;

        foreach (QuestObjectiveData objective in quest.Data.objectives)
        {
            if (objective == null)
                continue;

            if (objective.objectiveID == "submit_quest")
                submitObjective = objective;
            else if (collectionObjective == null)
                collectionObjective = objective;
        }

        if (submitObjective == null || collectionObjective == null)
            return;

        bool requirementsAvailable =
            HasRequirements(quest.Data, out _);

        QuestObjectiveData desiredObjective = requirementsAvailable
            ? submitObjective
            : collectionObjective;

        if (quest.CurrentObjectiveID == desiredObjective.objectiveID)
            return;

        foreach (QuestObjective objective in quest.Objectives)
        {
            bool isCollection =
                objective.ObjectiveID == collectionObjective.objectiveID;
            bool isDesired =
                objective.ObjectiveID == desiredObjective.objectiveID;

            objective.Completed = requirementsAvailable && isCollection;

            if (isDesired)
                objective.Text = desiredObjective.FormatProgress(0);
        }

        quest.CurrentObjectiveID = desiredObjective.objectiveID;
        RebuildAssetObjectiveHistory(quest);
        RaiseUpdated(quest);
        ObjectivesUI.Instance?.RefreshDisplayedQuest();
    }

    public bool CanShowSideQuest(
        QuestData questData)
    {
        if (questData == null ||
            questData.category != QuestCategory.Side ||
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
        trackedQuestState = FindSideQuestState(questData);

        RefreshSceneQuestMarkers();
        RaiseUpdated(trackedQuestState);
    }

    public void TrackQuest(QuestState quest)
    {
        if (quest == null || quest.Completed)
            return;

        bool isSideQuest = quest.Data != null &&
                           quest.Data.category == QuestCategory.Side;

        if (isSideQuest && !HasSideQuest(quest.Data))
            return;

        trackedQuestState = quest;
        trackedQuestData = isSideQuest ? quest.Data : null;

        RefreshSceneQuestMarkers();
        RaiseUpdated(quest);
    }

    public QuestState GetDisplayedQuest()
    {
        if (trackedQuestState != null && !trackedQuestState.Completed)
            return trackedQuestState;

        trackedQuestState = null;
        trackedQuestData = null;

        QuestState latestMainQuest = currentMainQuest;

        foreach (QuestState quest in objectiveJournalQuests)
        {
            if (quest == null || quest.Completed)
                continue;

            if (latestMainQuest == null ||
                quest.LastUpdatedOrder > latestMainQuest.LastUpdatedOrder)
            {
                latestMainQuest = quest;
            }
        }

        return latestMainQuest;
    }

    private QuestState FindSideQuestState(QuestData questData)
    {
        foreach (QuestState sideQuest in sideQuests)
        {
            if (sideQuest.Data == questData)
                return sideQuest;
        }

        return null;
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
        trackedQuestState = null;

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
            trackedQuestState = null;
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
        QuestState quest = updatedQuest ?? currentMainQuest;

        if (quest != null)
            quest.LastUpdatedOrder = ++questUpdateOrder;

        OnQuestUpdated?.Invoke(
            quest
        );
    }
}
