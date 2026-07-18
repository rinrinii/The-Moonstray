using UnityEngine;

public class PinewatchTutorialController : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private GameObject southExitBlocker;

    [SerializeField] private Transform playerSpawn;

    private void Start()
    {
        if (TutorialManager.Instance == null)
            return;

        TutorialManager.Instance.OnStateChanged += HandleStateChanged;

        if (TutorialManager.Instance.CurrentState == TutorialState.None)
        {
            TutorialManager.Instance.StartTutorial();
        }

        HandleStateChanged(TutorialManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.PinewatchTrail:
                EnterPinewatch();
                break;

            case TutorialState.SnowCourtyard:
                ExitPinewatch();
                break;
        }
    }

    private void EnterPinewatch()
    {
        Debug.Log("Entered Pinewatch Tutorial");

        // Prevent leaving the area
        if (southExitBlocker != null)
            southExitBlocker.SetActive(true);

        // Player starts as a wolf
        PlayerTransformation.Instance?.ForceWolfForm();
        PlayerTransformation.Instance?.LockTransformation();

        // Hide unavailable HUD icons
        HUDController.Instance?.SetBottomRightHUDVisible(false);

        // Initial objective
        ObjectivesUI.Instance?.SetObjective(
            "tutorial.finding_your_footing",
            "explore_trail",
            0);

        // Prompt is now handled by MovementTutorialTriggers.
        PromptUI.Instance?.Hide();
    }

    private void ExitPinewatch()
    {
        Debug.Log("Leaving Pinewatch Tutorial");

        if (southExitBlocker != null)
            southExitBlocker.SetActive(false);

        PromptUI.Instance?.Hide();

        ObjectivesUI.Instance?.Clear();
    }
}
