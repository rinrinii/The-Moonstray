using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class QuestCompassIndicator : MonoBehaviour
{
    public static QuestCompassIndicator Instance { get; private set; }

    [SerializeField] private Transform player;
    private Transform activeQuestTarget;
    [SerializeField] private float rotationOffset;

    private QuestObjectiveData trackedObjective;
    private const float PulseDuration = 0.8f;
    private const float PulseScale = 0.16f;
    private float pulseEndTime;

    private VisualElement minimapContainer;
    private VisualElement compassArrow;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void LateUpdate()
    {
        HandleTrackingInput();
        RefreshTrackedObjective();
        UpdateIndicator();
    }

    private void HandleTrackingInput()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.vKey.wasPressedThisFrame)
        {
            return;
        }

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        QuestObjectiveData currentObjective =
            ObjectivesUI.Instance?.CurrentObjectiveData;

        if (currentObjective == null ||
            currentObjective.trackingMode == ObjectiveTrackingMode.None)
        {
            StopTracking();
            return;
        }

        trackedObjective = currentObjective;
        ResolveObjectiveTarget();

        if (activeQuestTarget != null)
            pulseEndTime = Time.unscaledTime + PulseDuration;
    }

    private void RefreshTrackedObjective()
    {
        QuestObjectiveData currentObjective =
            ObjectivesUI.Instance?.CurrentObjectiveData;

        if (currentObjective == null ||
            currentObjective.trackingMode == ObjectiveTrackingMode.None)
        {
            StopTracking();
            return;
        }

        if (currentObjective != trackedObjective)
        {
            trackedObjective = currentObjective;
        }

        ResolveObjectiveTarget();
    }

    private void ResolveObjectiveTarget()
    {
        activeQuestTarget = null;

        if (trackedObjective == null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;
        string targetScene = trackedObjective.targetScene;

        if (!string.IsNullOrWhiteSpace(targetScene) &&
            currentScene != targetScene)
        {
            activeQuestTarget = FindRouteTarget(currentScene, targetScene);
            return;
        }

        if (trackedObjective.trackingMode == ObjectiveTrackingMode.SceneExit ||
            string.IsNullOrWhiteSpace(trackedObjective.trackingMarkerID))
        {
            return;
        }

        MapMarkerTarget marker = MapMarkerTarget.FindByID(
            trackedObjective.trackingMarkerID);

        if (marker != null && marker.gameObject.activeInHierarchy)
            activeQuestTarget = marker.transform;
    }

    private static Transform FindRouteTarget(
        string currentScene,
        string targetScene)
    {
        SceneTransitionTrigger[] transitions =
            Object.FindObjectsByType<SceneTransitionTrigger>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

        foreach (SceneTransitionTrigger transition in transitions)
        {
            if (transition.TargetScene == targetScene)
                return transition.CompassTarget;
        }

        string nextScene = SceneRouteDatabase.FindNextScene(
            currentScene,
            targetScene);

        if (string.IsNullOrWhiteSpace(nextScene))
            return null;

        foreach (SceneTransitionTrigger transition in transitions)
        {
            if (transition.TargetScene == nextScene)
                return transition.CompassTarget;
        }

        return null;
    }

    private void RefreshReferences()
    {
        VisualElement root =
            GameplayUIManager.Instance?.RootVisualElement;

        if (root == null)
            return;

        minimapContainer =
            root.Q<VisualElement>("MinimapWrapper");

        compassArrow =
            root.Q<VisualElement>("QuestCompassArrow");
    }

    private void UpdateIndicator()
    {
        if (minimapContainer == null ||
            compassArrow == null)
        {
            RefreshReferences();

            if (minimapContainer == null ||
                compassArrow == null)
            {
                return;
            }
        }

        if (player == null ||
            activeQuestTarget == null)
        {
            compassArrow.style.display =
                DisplayStyle.None;

            return;
        }

        float width =
            minimapContainer.resolvedStyle.width;

        float height =
            minimapContainer.resolvedStyle.height;

        if (width <= 0f ||
            height <= 0f)
        {
            return;
        }

        Vector3 direction =
            activeQuestTarget.position -
            player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            compassArrow.style.display =
                DisplayStyle.None;

            return;
        }

        if (trackedObjective != null &&
            trackedObjective.trackingMode == ObjectiveTrackingMode.SearchArea &&
            trackedObjective.hideInsideArea &&
            direction.sqrMagnitude <=
                trackedObjective.areaRadius * trackedObjective.areaRadius)
        {
            compassArrow.style.display = DisplayStyle.None;
            return;
        }

        compassArrow.style.display =
            DisplayStyle.Flex;

        float angle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg +
            rotationOffset;

        compassArrow.style.transformOrigin =
            new TransformOrigin(
                Length.Percent(50),
                Length.Percent(50),
                0
            );

        compassArrow.style.rotate =
            new Rotate(
                new Angle(
                    angle,
                    AngleUnit.Degree
                )
            );

        ApplyPulse();
    }

    public void SetActiveQuestTarget(
        Transform target)
    {
        activeQuestTarget = target;
    }

    public void ClearActiveQuestTarget()
    {
        StopTracking();
    }

    private void StopTracking()
    {
        trackedObjective = null;
        activeQuestTarget = null;

        if (compassArrow != null)
        {
            compassArrow.style.display = DisplayStyle.None;
            compassArrow.style.scale = new Scale(Vector2.one);
            compassArrow.style.opacity = 1f;
        }
    }

    private void ApplyPulse()
    {
        if (compassArrow == null)
            return;

        float remaining = pulseEndTime - Time.unscaledTime;

        if (remaining <= 0f)
        {
            compassArrow.style.scale = new Scale(Vector2.one);
            compassArrow.style.opacity = 1f;
            return;
        }

        float progress = 1f - remaining / PulseDuration;
        float wave = Mathf.Sin(progress * Mathf.PI * 2f);
        float pulse = wave * wave;
        float scale = 1f + pulse * PulseScale;

        compassArrow.style.scale =
            new Scale(new Vector2(scale, scale));

        compassArrow.style.opacity = 0.78f + pulse * 0.22f;
    }
}
