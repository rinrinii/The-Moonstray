using UnityEngine;

public class PinewatchTutorialController : MonoBehaviour
{
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

        Debug.Log("PlayerTransformation = " + PlayerTransformation.Instance);
        Debug.Log("HUDController = " + HUDController.Instance);
        Debug.Log("ObjectivesUI = " + ObjectivesUI.Instance);
        Debug.Log("PromptUI = " + PromptUI.Instance);

        PlayerTransformation.Instance?.ForceWolfForm();
        PlayerTransformation.Instance?.LockTransformation();

        HUDController.Instance?.SetBottomRightHUDVisible(false);

        ObjectivesUI.Instance?.SetObjective(
            "Finding Your Footing",
            "Explore the trail."
        );

        PromptUI.Instance?.Show(
            "[LEFT SHIFT]",
            "Hold Left Shift to sprint."
        );
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