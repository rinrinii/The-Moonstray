using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement panel;
    private VisualElement hudGroup;

    private Label titleLabel;
    private Label descriptionLabel;
    private VisualElement trackingHint;
    private VisualElement trackingKeyIcon;
    private Label trackingHintLabel;
    private QuestManager subscribedQuestManager;
    private Coroutine objectiveTransition;
    private string displayedQuestKey;
    private string displayedObjectiveID;
    private bool showingObjectiveCompletion;
    private readonly Queue<ObjectivePresentation> pendingObjectiveTransitions = new();
    private readonly Queue<string> pendingQuestCompletions = new();
    private string completionStatusText = "Completed";

    private readonly struct ObjectivePresentation
    {
        public readonly string Title;
        public readonly string Objective;

        public ObjectivePresentation(string title, string objective)
        {
            Title = title;
            Objective = objective;
        }
    }

    public QuestObjectiveData CurrentObjectiveData { get; private set; }

    public static ObjectivesUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize(VisualElement root)
    {
        panel = root.Q<VisualElement>("ObjectivesPanel");
        hudGroup = root.Q<VisualElement>("ObjectivesHUDGroup");

        titleLabel =
            root.Q<Label>("MainQuestTitle");

        descriptionLabel =
            root.Q<Label>("MainQuestDescription");

        trackingHint = root.Q<VisualElement>("ObjectiveTrackingHint");
        trackingKeyIcon = root.Q<VisualElement>("ObjectiveTrackingKeyIcon");
        trackingHintLabel = root.Q<Label>("ObjectiveTrackingHintLabel");

        Hide();

        EnsureQuestSubscription();
    }

    private void OnDestroy()
    {
        if (subscribedQuestManager != null)
        {
            subscribedQuestManager.OnQuestUpdated -= Refresh;
            subscribedQuestManager.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void Update()
    {
        EnsureQuestSubscription();
        RestoreMissingDisplayedText();
        RefreshTrackingHint();
    }

    private void RestoreMissingDisplayedText()
    {
        if (showingObjectiveCompletion || objectiveTransition != null ||
            titleLabel == null || descriptionLabel == null)
        {
            return;
        }

        QuestState quest = QuestManager.Instance?.GetDisplayedQuest();
        if (quest == null)
            return;

        // Loading screens and scene bootstraps can temporarily clear the
        // persistent HUD after quest state has already been restored. Repair
        // that presentation drift without modifying quest progress.
        if (string.IsNullOrWhiteSpace(titleLabel.text) ||
            string.IsNullOrWhiteSpace(descriptionLabel.text))
        {
            Refresh(quest);
        }
    }

    private void EnsureQuestSubscription()
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == subscribedQuestManager)
            return;

        if (subscribedQuestManager != null)
        {
            subscribedQuestManager.OnQuestUpdated -= Refresh;
            subscribedQuestManager.OnQuestCompleted -= HandleQuestCompleted;
        }

        subscribedQuestManager = manager;
        if (subscribedQuestManager == null)
            return;

        subscribedQuestManager.OnQuestUpdated += Refresh;
        subscribedQuestManager.OnQuestCompleted += HandleQuestCompleted;
        Refresh(subscribedQuestManager.GetDisplayedQuest());
    }

    private void RefreshTrackingHint()
    {
        if (trackingHint == null)
            return;

        if (showingObjectiveCompletion)
        {
            trackingHint.style.display = DisplayStyle.Flex;
            trackingKeyIcon.style.display = DisplayStyle.None;
            trackingHintLabel.text = completionStatusText;
            trackingHintLabel.style.color = new Color(1f, 0.84f, 0.3f, 1f);
            return;
        }

        trackingHintLabel.style.color = new Color(1f, 1f, 1f, 0.82f);

        QuestObjectiveData objective = CurrentObjectiveData;

        if (objective == null)
        {
            trackingHint.style.display = DisplayStyle.None;
            return;
        }

        if (objective.trackingMode == ObjectiveTrackingMode.None)
        {
            bool isGeneralCollection =
                objective.type == QuestObjectiveType.Collect;

            trackingHint.style.display = isGeneralCollection
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            if (isGeneralCollection)
            {
                trackingKeyIcon.style.display = DisplayStyle.Flex;
                bool isInPossibleArea =
                    objective.possibleScenes != null &&
                    objective.possibleScenes.Contains(
                        SceneManager.GetActiveScene().name);

                trackingHintLabel.text = isInPossibleArea
                    ? "Currently in tracked location"
                    : "Check journal for possible areas";
            }

            return;
        }

        trackingHint.style.display = DisplayStyle.Flex;

        bool isInsideSearchArea =
            objective.trackingMode == ObjectiveTrackingMode.SearchArea &&
            QuestCompassIndicator.Instance != null &&
            QuestCompassIndicator.Instance.IsInsideTrackedArea;

        trackingKeyIcon.style.display = DisplayStyle.Flex;

        trackingHintLabel.text = isInsideSearchArea
            ? "Currently in tracked location"
            : "Track current objective";
    }

    private void Refresh(QuestState quest)
    {
        quest = QuestManager.Instance?.GetDisplayedQuest() ?? quest;

        if (quest == null)
        {
            Hide();
            return;
        }

        Show();

        if (quest.IsObjectiveLog)
        {
            CurrentObjectiveData = FindCurrentObjectiveData(quest);
            string questKey = GetQuestKey(quest);
            string objectiveID = quest.CurrentObjectiveID;
            bool objectiveAdvanced =
                !string.IsNullOrWhiteSpace(displayedQuestKey) &&
                displayedQuestKey == questKey &&
                !string.IsNullOrWhiteSpace(displayedObjectiveID) &&
                !string.IsNullOrWhiteSpace(objectiveID) &&
                displayedObjectiveID != objectiveID;

            displayedQuestKey = questKey;
            displayedObjectiveID = objectiveID;

            if (objectiveAdvanced)
                BeginObjectiveTransition(quest.Title, quest.CurrentObjectiveText);
            else
                SetDisplayedText(quest.Title, quest.CurrentObjectiveText);

            return;
        }

        CurrentObjectiveData = null;
        displayedQuestKey = GetQuestKey(quest);
        displayedObjectiveID = string.Empty;

        StringBuilder builder = new();

        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            builder.AppendLine(quest.Objectives[i].Text);
        }

        SetDisplayedText(quest.Title, builder.ToString());
    }

    public void Show()
    {
        if (panel == null)
            return;

        hudGroup.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (panel == null)
            return;

        StopObjectiveTransition();
        titleLabel.text = string.Empty;
        descriptionLabel.text = string.Empty;
        CurrentObjectiveData = null;
        displayedQuestKey = string.Empty;
        displayedObjectiveID = string.Empty;

        if (trackingHint != null)
            trackingHint.style.display = DisplayStyle.None;

        hudGroup.style.display = DisplayStyle.None;
    }

    public void Clear()
    {
        QuestState activeQuest = QuestManager.Instance?.GetDisplayedQuest();
        if (activeQuest != null)
        {
            // Scene and tutorial transitions may request a clear immediately
            // before reporting the next objective. Keep the last valid state
            // visible so the HUD never flashes as an empty frame.
            Show();
            return;
        }

        Hide();
    }

    public void RefreshDisplayedQuest()
    {
        Refresh(QuestManager.Instance?.GetDisplayedQuest());
    }

    private void BeginObjectiveTransition(string title, string objective)
    {
        if (objectiveTransition != null)
        {
            pendingObjectiveTransitions.Enqueue(
                new ObjectivePresentation(title, objective));
            return;
        }

        objectiveTransition = StartCoroutine(
            PlayObjectiveTransition(title, objective));
    }

    private IEnumerator PlayObjectiveTransition(string title, string objective)
    {
        yield return WaitUntilHudAvailable();
        showingObjectiveCompletion = true;
        completionStatusText = "Completed";
        RefreshTrackingHint();
        descriptionLabel.style.opacity = 1f;
        descriptionLabel.AddToClassList("objective-complete-glow");
        yield return new WaitForSecondsRealtime(0.55f);

        const float fadeOutDuration = 0.22f;
        const float crossfadeFloor = 0.25f;
        for (float elapsed = 0f; elapsed < fadeOutDuration; elapsed += Time.unscaledDeltaTime)
        {
            descriptionLabel.style.opacity = Mathf.Lerp(
                1f,
                crossfadeFloor,
                elapsed / fadeOutDuration);
            yield return null;
        }

        descriptionLabel.RemoveFromClassList("objective-complete-glow");
        titleLabel.text = title;
        descriptionLabel.text = objective;
        descriptionLabel.style.opacity = crossfadeFloor;

        const float fadeInDuration = 0.45f;
        for (float elapsed = 0f; elapsed < fadeInDuration; elapsed += Time.unscaledDeltaTime)
        {
            descriptionLabel.style.opacity = Mathf.Lerp(
                crossfadeFloor,
                1f,
                elapsed / fadeInDuration);
            yield return null;
        }

        descriptionLabel.style.opacity = 1f;
        showingObjectiveCompletion = false;
        RefreshTrackingHint();
        objectiveTransition = null;
        PlayNextQueuedPresentation();
    }

    private void HandleQuestCompleted(string questTitle)
    {
        if (objectiveTransition != null)
        {
            pendingQuestCompletions.Enqueue(questTitle);
            return;
        }

        objectiveTransition = StartCoroutine(
            PlayQuestCompleteTransition(questTitle));
    }

    private IEnumerator PlayQuestCompleteTransition(string questTitle)
    {
        yield return WaitUntilHudAvailable();
        Show();
        titleLabel.text = questTitle;
        descriptionLabel.style.opacity = 1f;
        descriptionLabel.AddToClassList("objective-complete-glow");
        showingObjectiveCompletion = true;
        completionStatusText = "Quest Complete";
        RefreshTrackingHint();
        yield return new WaitForSecondsRealtime(1.1f);

        descriptionLabel.RemoveFromClassList("objective-complete-glow");
        showingObjectiveCompletion = false;
        objectiveTransition = null;

        QuestState nextQuest = QuestManager.Instance?.GetDisplayedQuest();
        if (nextQuest != null)
            Refresh(nextQuest);
        else
            Hide();

        PlayNextQueuedPresentation();
    }

    private void PlayNextQueuedPresentation()
    {
        if (objectiveTransition != null)
            return;

        if (pendingObjectiveTransitions.Count > 0)
        {
            ObjectivePresentation next = pendingObjectiveTransitions.Dequeue();
            BeginObjectiveTransition(next.Title, next.Objective);
            return;
        }

        if (pendingQuestCompletions.Count > 0)
            HandleQuestCompleted(pendingQuestCompletions.Dequeue());
    }

    private IEnumerator WaitUntilHudAvailable()
    {
        while (GameplayUIManager.Instance?.HudContainer != null &&
               GameplayUIManager.Instance.HudContainer.resolvedStyle.display ==
               DisplayStyle.None)
        {
            yield return null;
        }
    }

    private void SetDisplayedText(string title, string objective)
    {
        StopObjectiveTransition();
        titleLabel.text = title;
        descriptionLabel.text = objective;
        descriptionLabel.style.opacity = 1f;
    }

    private void StopObjectiveTransition()
    {
        if (objectiveTransition != null)
        {
            StopCoroutine(objectiveTransition);
            objectiveTransition = null;
        }

        if (descriptionLabel != null)
        {
            descriptionLabel.RemoveFromClassList("objective-complete-glow");
            descriptionLabel.style.opacity = 1f;
        }

        showingObjectiveCompletion = false;
        pendingObjectiveTransitions.Clear();
        pendingQuestCompletions.Clear();
        completionStatusText = "Completed";
        if (trackingHintLabel != null)
            trackingHintLabel.style.color = new Color(1f, 1f, 1f, 0.82f);
    }

    private static string GetQuestKey(QuestState quest)
    {
        if (!string.IsNullOrWhiteSpace(quest?.Data?.questID))
            return quest.Data.questID;

        return quest?.Title ?? string.Empty;
    }

    private static QuestObjectiveData FindCurrentObjectiveData(QuestState quest)
    {
        if (quest?.Data?.objectives == null)
            return null;

        foreach (QuestObjectiveData objective in quest.Data.objectives)
        {
            if (objective != null &&
                objective.objectiveID == quest.CurrentObjectiveID)
            {
                return objective;
            }
        }

        return null;
    }
}
