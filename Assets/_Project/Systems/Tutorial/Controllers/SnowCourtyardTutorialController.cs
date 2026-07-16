using UnityEngine;

public class SnowCourtyardTutorialController : MonoBehaviour
{
    [Header("Exit Blockers")]
    [SerializeField] private GameObject pinewatchExitBlocker;

    [SerializeField] private GameObject libraryExitBlocker;

    [SerializeField] private GameObject southExitBlocker;

    [SerializeField]
    private int requiredInspections = 2;

    private int inspectionsCompleted;
    private bool explorationComplete;

    public static SnowCourtyardTutorialController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (TutorialManager.Instance == null)
            return;

        TutorialManager.Instance.OnStateChanged += HandleStateChanged;

        HandleStateChanged(TutorialManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(TutorialState state)
    {
        SetExitBlockersActive(state == TutorialState.SnowCourtyard);

        switch (state)
        {
            case TutorialState.SnowCourtyard:
                EnterSnowCourtyard();
                break;

            case TutorialState.BlightPath:
                ExitSnowCourtyard();
                break;
        }
    }

    private void EnterSnowCourtyard()
    {
        Debug.Log("Entered Snow Courtyard Tutorial");

        // TEMPORARILY REMOVE THESE
        // PlayerTransformation.Instance?.ForceWolfForm();
        // PlayerTransformation.Instance?.LockTransformation();

        Debug.Log("After ForceWolfForm");

        HUDController.Instance?.SetBottomRightHUDVisible(false);

        Debug.Log("Before Objective");

        ObjectivesUI.Instance?.SetObjective(
            "Snow Courtyard",
            "Explore the courtyard."
        );

        PromptUI.Instance?.Show(
            "[E] Interact",
            "Press E to inspect objects."
        );

        Debug.Log("Objective set");
    }

    private void ExitSnowCourtyard()
    {
        Debug.Log("Leaving Snow Courtyard Tutorial");

        PromptUI.Instance?.Hide();
        ObjectivesUI.Instance?.Clear();
    }

    private void SetExitBlockersActive(bool active)
    {
        if (pinewatchExitBlocker != null)
            pinewatchExitBlocker.SetActive(active);

        if (libraryExitBlocker != null)
            libraryExitBlocker.SetActive(active);

        if (southExitBlocker != null)
            southExitBlocker.SetActive(active);
    }

    public void RegisterInspection()
    {
        if (explorationComplete)
            return;

        inspectionsCompleted++;

        // The interaction prompt has served its purpose once the player
        // successfully inspects an object for the first time.
        if (inspectionsCompleted == 1)
            PromptUI.Instance?.Hide();

        if (inspectionsCompleted < requiredInspections)
        {
            ObjectivesUI.Instance?.SetObjective(
                "Searching for Clues",
                $"Explore the courtyard ({inspectionsCompleted}/{requiredInspections})"
            );

            return;
        }

        explorationComplete = true;

        ObjectivesUI.Instance?.SetObjective(
            "Searching for Clues",
            "Leave the courtyard."
        );

    }
}
