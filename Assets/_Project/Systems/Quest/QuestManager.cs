using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour
{
    [Serializable]
    public sealed class Snapshot
    {
        public QuestRecord currentMainQuest;
        public List<QuestRecord> sideQuests = new();
        public List<string> completedSideQuestIDs = new();
        public List<QuestRecord> objectiveJournalQuests = new();
        public string trackedQuestID;
        public string trackedQuestTitle;
        public string currentObjectiveJournalQuestID;
        public string currentObjectiveJournalQuestTitle;
        public int currentObjectiveIndex;
        public long updateOrder;
    }

    [Serializable]
    public sealed class QuestRecord
    {
        public string questID;
        public string title;
        public string description;
        public string conditions;
        public string rewards;
        public bool completed;
        public bool isObjectiveLog;
        public long lastUpdatedOrder;
        public string currentObjectiveID;
        public List<ObjectiveRecord> objectives = new();
    }

    [Serializable]
    public sealed class ObjectiveRecord
    {
        public string objectiveID;
        public string text;
        public bool completed;
        public int currentAmount;
        public int requiredAmount = 1;
        public List<string> completedStepIDs = new();
    }

    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestUpdated;
    public event Action<string> OnQuestCompleted;

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

    public void ResetProgress()
    {
        currentMainQuest = null;
        sideQuests.Clear();
        completedSideQuests.Clear();
        objectiveJournalQuests.Clear();
        trackedQuestData = null;
        trackedQuestState = null;
        currentObjectiveJournalQuest = null;
        CurrentObjectiveIndex = 0;
        questUpdateOrder = 0;

        RefreshSceneQuestMarkers();
        RaiseUpdated();
    }

    public Snapshot CaptureState()
    {
        Snapshot snapshot = new()
        {
            currentMainQuest = CaptureQuest(currentMainQuest),
            trackedQuestID = GetQuestID(trackedQuestState),
            trackedQuestTitle = trackedQuestState?.Title,
            currentObjectiveJournalQuestID =
                GetQuestID(currentObjectiveJournalQuest),
            currentObjectiveJournalQuestTitle =
                currentObjectiveJournalQuest?.Title,
            currentObjectiveIndex = CurrentObjectiveIndex,
            updateOrder = questUpdateOrder
        };

        foreach (QuestState quest in sideQuests)
            snapshot.sideQuests.Add(CaptureQuest(quest));

        foreach (QuestData quest in completedSideQuests)
        {
            if (quest != null && !string.IsNullOrWhiteSpace(quest.questID))
                snapshot.completedSideQuestIDs.Add(quest.questID);
        }

        snapshot.completedSideQuestIDs.Sort(StringComparer.Ordinal);

        foreach (QuestState quest in objectiveJournalQuests)
            snapshot.objectiveJournalQuests.Add(CaptureQuest(quest));

        return snapshot;
    }

    public void RestoreState(Snapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("Cannot restore null quest state.");
            return;
        }

        currentMainQuest = RestoreQuest(snapshot.currentMainQuest);
        sideQuests.Clear();
        completedSideQuests.Clear();
        objectiveJournalQuests.Clear();
        trackedQuestData = null;
        trackedQuestState = null;
        currentObjectiveJournalQuest = null;

        if (snapshot.sideQuests != null)
        {
            foreach (QuestRecord record in snapshot.sideQuests)
            {
                QuestState quest = RestoreQuest(record);
                if (quest != null)
                    sideQuests.Add(quest);
            }
        }

        if (snapshot.completedSideQuestIDs != null)
        {
            foreach (string questID in snapshot.completedSideQuestIDs)
            {
                QuestData quest = FindQuestData(questID);
                if (quest != null)
                    completedSideQuests.Add(quest);
            }
        }

        if (snapshot.objectiveJournalQuests != null)
        {
            foreach (QuestRecord record in snapshot.objectiveJournalQuests)
            {
                QuestState quest = RestoreQuest(record);
                if (quest != null)
                    objectiveJournalQuests.Add(quest);
            }
        }

        trackedQuestState = FindRestoredQuest(
            snapshot.trackedQuestID,
            snapshot.trackedQuestTitle);
        trackedQuestData = trackedQuestState?.Data?.category == QuestCategory.Side
            ? trackedQuestState.Data
            : null;

        currentObjectiveJournalQuest = FindRestoredQuest(
            snapshot.currentObjectiveJournalQuestID,
            snapshot.currentObjectiveJournalQuestTitle,
            objectiveJournalQuests);

        CurrentObjectiveIndex = Mathf.Max(0, snapshot.currentObjectiveIndex);
        questUpdateOrder = Math.Max(0, snapshot.updateOrder);

        RefreshSceneQuestMarkers();
        OnQuestUpdated?.Invoke(GetDisplayedQuest());
    }

    private static QuestRecord CaptureQuest(QuestState quest)
    {
        if (quest == null)
            return null;

        QuestRecord record = new()
        {
            questID = GetQuestID(quest),
            title = quest.Title,
            description = quest.Description,
            conditions = quest.Conditions,
            rewards = quest.Rewards,
            completed = quest.Completed,
            isObjectiveLog = quest.IsObjectiveLog,
            lastUpdatedOrder = quest.LastUpdatedOrder,
            currentObjectiveID = quest.CurrentObjectiveID
        };

        foreach (QuestObjective objective in quest.Objectives)
        {
            record.objectives.Add(new ObjectiveRecord
            {
                objectiveID = objective.ObjectiveID,
                text = objective.Text,
                completed = objective.Completed,
                currentAmount = objective.CurrentAmount,
                requiredAmount = objective.RequiredAmount,
                completedStepIDs = objective.CompletedStepIDs != null
                    ? new List<string>(objective.CompletedStepIDs)
                    : new List<string>()
            });
        }

        return record;
    }

    private QuestState RestoreQuest(QuestRecord record)
    {
        if (record == null)
            return null;

        QuestData data = string.IsNullOrWhiteSpace(record.questID)
            ? null
            : FindQuestData(record.questID);

        if (!string.IsNullOrWhiteSpace(record.questID) && data == null)
        {
            Debug.LogWarning(
                $"Saved quest '{record.questID}' is not available in this build.");
            return null;
        }

        QuestState quest = data != null
            ? new QuestState(data)
            : new QuestState(record.title);

        quest.Title = record.title;
        quest.Description = record.description;
        quest.Conditions = record.conditions;
        quest.Rewards = record.rewards;
        quest.Completed = record.completed;
        quest.IsObjectiveLog = record.isObjectiveLog;
        quest.LastUpdatedOrder = record.lastUpdatedOrder;
        quest.CurrentObjectiveID = record.currentObjectiveID;
        quest.Objectives.Clear();

        if (record.objectives != null)
        {
            foreach (ObjectiveRecord objective in record.objectives)
            {
                if (objective == null)
                    continue;

                QuestObjective restoredObjective = new(objective.text)
                {
                    ObjectiveID = objective.objectiveID,
                    Completed = objective.completed,
                    CurrentAmount = Mathf.Max(0, objective.currentAmount),
                    RequiredAmount = Mathf.Max(1, objective.requiredAmount)
                };

                if (objective.completedStepIDs != null)
                    restoredObjective.CompletedStepIDs.AddRange(
                        objective.completedStepIDs);

                quest.Objectives.Add(restoredObjective);
            }
        }

        return quest;
    }

    private QuestState FindRestoredQuest(
        string questID,
        string title,
        IReadOnlyList<QuestState> preferredList = null)
    {
        if (preferredList != null)
            return FindRestoredQuestInList(preferredList, questID, title);

        if (QuestMatches(currentMainQuest, questID, title))
            return currentMainQuest;

        QuestState quest = FindRestoredQuestInList(sideQuests, questID, title);
        return quest ?? FindRestoredQuestInList(
            objectiveJournalQuests,
            questID,
            title);
    }

    private static QuestState FindRestoredQuestInList(
        IReadOnlyList<QuestState> quests,
        string questID,
        string title)
    {
        foreach (QuestState quest in quests)
        {
            if (QuestMatches(quest, questID, title))
                return quest;
        }

        return null;
    }

    private static bool QuestMatches(
        QuestState quest,
        string questID,
        string title)
    {
        if (quest == null)
            return false;

        if (!string.IsNullOrWhiteSpace(questID))
            return GetQuestID(quest) == questID;

        return quest.Title == title;
    }

    private static string GetQuestID(QuestState quest)
    {
        return quest?.Data?.questID ?? string.Empty;
    }

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

    /// <summary>
    /// Records a legacy, text-only objective. Gameplay code should report
    /// progression to QuestManager rather than using ObjectivesUI as a command
    /// surface. The UI observes OnQuestUpdated and only renders the result.
    /// </summary>
    public QuestState SetObjective(string title, string description)
    {
        return RecordObjectiveForJournal(title, description);
    }

    /// <summary>
    /// Activates or updates an authored objective by stable IDs.
    /// </summary>
    public QuestState SetObjective(
        string questID,
        string objectiveID,
        int currentAmount = 0)
    {
        return ActivateObjective(questID, objectiveID, currentAmount);
    }

    public int AddObjectiveProgress(
        string questID,
        string objectiveID,
        int amount = 1)
    {
        QuestState quest = FindObjectiveJournalQuest(questID) ??
            ActivateObjective(questID, objectiveID, 0);

        if (quest == null || quest.CurrentObjectiveID != objectiveID)
            return 0;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID != objectiveID)
                continue;

            int required = Mathf.Max(1, objective.RequiredAmount);
            objective.CurrentAmount = Mathf.Clamp(
                objective.CurrentAmount + amount,
                0,
                required);
            objective.Completed = objective.CurrentAmount >= required;

            QuestObjectiveData data = FindObjectiveData(
                quest.Data,
                objectiveID);
            if (data != null)
                objective.Text = data.FormatProgress(objective.CurrentAmount);

            RebuildAssetObjectiveHistory(quest);
            RaiseUpdated(quest);
            return objective.CurrentAmount;
        }

        return 0;
    }

    public int GetObjectiveProgress(string questID, string objectiveID)
    {
        QuestState quest = FindObjectiveJournalQuest(questID);
        if (quest == null)
            return 0;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID == objectiveID)
                return objective.CurrentAmount;
        }

        return 0;
    }

    public bool IsObjectiveComplete(string questID, string objectiveID)
    {
        QuestState quest = FindObjectiveJournalQuest(questID);
        if (quest == null)
            return false;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID == objectiveID)
                return objective.Completed;
        }

        return false;
    }

    public string GetCurrentObjectiveID(string questID)
    {
        return FindObjectiveJournalQuest(questID)?.CurrentObjectiveID ??
            string.Empty;
    }

    public bool CompleteObjectiveStep(
        string questID,
        string objectiveID,
        string stepID)
    {
        if (string.IsNullOrWhiteSpace(stepID))
            return false;

        QuestState quest = FindObjectiveJournalQuest(questID) ??
            ActivateObjective(questID, objectiveID, 0);
        if (quest == null)
            return false;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID != objectiveID ||
                objective.CompletedStepIDs.Contains(stepID))
            {
                continue;
            }

            objective.CompletedStepIDs.Add(stepID);
            objective.CompletedStepIDs.Sort(StringComparer.Ordinal);
            objective.CurrentAmount = Mathf.Clamp(
                objective.CompletedStepIDs.Count,
                0,
                Mathf.Max(1, objective.RequiredAmount));
            objective.Completed =
                quest.CurrentObjectiveID == objectiveID &&
                objective.CurrentAmount >= Mathf.Max(1, objective.RequiredAmount);

            QuestObjectiveData data = FindObjectiveData(
                quest.Data,
                objectiveID);
            if (data != null)
                objective.Text = data.FormatProgress(objective.CurrentAmount);

            RebuildAssetObjectiveHistory(quest);
            RaiseUpdated(quest);
            return true;
        }

        return false;
    }

    public bool HasCompletedObjectiveStep(
        string questID,
        string objectiveID,
        string stepID)
    {
        QuestState quest = FindObjectiveJournalQuest(questID);
        if (quest == null)
            return false;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID == objectiveID)
                return objective.CompletedStepIDs.Contains(stepID);
        }

        return false;
    }

    public int GetCompletedObjectiveStepCount(
        string questID,
        string objectiveID)
    {
        QuestState quest = FindObjectiveJournalQuest(questID);
        if (quest == null)
            return 0;

        foreach (QuestObjective objective in quest.Objectives)
        {
            if (objective.ObjectiveID == objectiveID)
                return objective.CompletedStepIDs.Count;
        }

        return 0;
    }

    private QuestState FindObjectiveJournalQuest(string questID)
    {
        if (string.IsNullOrWhiteSpace(questID))
            return null;

        foreach (QuestState quest in objectiveJournalQuests)
        {
            if (GetQuestID(quest) == questID)
                return quest;
        }

        return null;
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

        QuestObjectiveData objectiveData = FindObjectiveData(data, objectiveID);

        if (objectiveData == null)
        {
            Debug.LogWarning(
                $"Objective '{objectiveID}' not found in quest '{questID}'.");
            return null;
        }

        RemoveLegacyObjectiveLog(data.DisplayTitle);

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

        int requestedObjectiveIndex = FindObjectiveIndex(data, objectiveID);
        int currentObjectiveIndex = FindObjectiveIndex(
            data,
            quest.CurrentObjectiveID);

        // Scene bootstraps run whenever an area is loaded. Once an authored
        // quest has advanced, a stale scene event must not rewind it.
        if (currentObjectiveIndex >= 0 &&
            requestedObjectiveIndex >= 0 &&
            requestedObjectiveIndex < currentObjectiveIndex)
        {
            return quest;
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

            objective.RequiredAmount = Mathf.Max(1, objectiveData.requiredAmount);
            objective.CurrentAmount = Mathf.Clamp(
                Mathf.Max(objective.CurrentAmount, currentAmount),
                0,
                objective.RequiredAmount);
            objective.Text = objectiveData.FormatProgress(
                objective.CurrentAmount);
            objective.Completed =
                objective.CurrentAmount >= objective.RequiredAmount;
            break;
        }

        quest.CurrentObjectiveID = objectiveID;
        quest.Completed = false;
        currentObjectiveJournalQuest = quest;

        RebuildAssetObjectiveHistory(quest);
        RaiseUpdated(quest);

        return quest;
    }

    private static QuestObjectiveData FindObjectiveData(
        QuestData data,
        string objectiveID)
    {
        if (data?.objectives == null)
            return null;

        foreach (QuestObjectiveData candidate in data.objectives)
        {
            if (candidate != null && candidate.objectiveID == objectiveID)
                return candidate;
        }

        return null;
    }

    private static int FindObjectiveIndex(
        QuestData data,
        string objectiveID)
    {
        if (data?.objectives == null ||
            string.IsNullOrWhiteSpace(objectiveID))
        {
            return -1;
        }

        for (int i = 0; i < data.objectives.Count; i++)
        {
            if (data.objectives[i]?.objectiveID == objectiveID)
                return i;
        }

        return -1;
    }

    public void AdvanceTravelObjectiveForDestination(string destinationScene)
    {
        QuestState quest = currentObjectiveJournalQuest;
        QuestData data = quest?.Data;

        if (data?.objectives == null ||
            string.IsNullOrWhiteSpace(quest.CurrentObjectiveID) ||
            string.IsNullOrWhiteSpace(destinationScene))
        {
            return;
        }

        for (int i = 0; i < data.objectives.Count; i++)
        {
            QuestObjectiveData objective = data.objectives[i];

            if (objective == null ||
                objective.objectiveID != quest.CurrentObjectiveID ||
                objective.type != QuestObjectiveType.Travel ||
                objective.targetScene != destinationScene ||
                i >= data.objectives.Count - 1)
            {
                continue;
            }

            QuestObjectiveData nextObjective = data.objectives[i + 1];
            if (nextObjective == null)
                return;

            ActivateObjective(
                data.questID,
                nextObjective.objectiveID,
                0);
            return;
        }
    }

    private void RemoveLegacyObjectiveLog(string title)
    {
        for (int i = objectiveJournalQuests.Count - 1; i >= 0; i--)
        {
            QuestState candidate = objectiveJournalQuests[i];
            if (candidate == null || candidate.Data != null ||
                candidate.Title != title)
            {
                continue;
            }

            if (currentObjectiveJournalQuest == candidate)
                currentObjectiveJournalQuest = null;
            if (trackedQuestState == candidate)
                trackedQuestState = null;

            objectiveJournalQuests.RemoveAt(i);
        }
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

        for (int i = quest.Objectives.Count - 1; i >= 0; i--)
        {
            QuestObjective objective = quest.Objectives[i];
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

        for (int i = quest.Objectives.Count - 1; i >= 0; i--)
        {
            QuestObjective objective = quest.Objectives[i];
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
        string completedTitle = questData != null
            ? questData.DisplayTitle
            : "Quest";
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
        OnQuestCompleted?.Invoke(completedTitle);
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
        string completedTitle = currentMainQuest?.Title ?? "Quest";
        currentMainQuest = null;
        CurrentObjectiveIndex = 0;

        OnQuestCompleted?.Invoke(completedTitle);
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
