using System.Collections;
using UnityEngine;

public class PinewatchTutorialController : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private GameObject southExitBlocker;

    [SerializeField] private Transform playerSpawn;

    [Header("Opening Sequence")]
    [SerializeField] private CutscenePlayer startCutscenePlayer;
    [SerializeField] private float wakingAnimationDuration = 2f;
    [SerializeField] private string wakingDialogueID = "intro.pinewatchWake";

    private readonly PlayerMovementFreezeHandle openingMovementLock = new();
    private bool openingSequenceStarted;

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

        openingMovementLock.Release();
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

        if (!openingSequenceStarted)
            StartCoroutine(PlayOpeningSequence());

        PromptUI.Instance?.Hide();
    }

    private IEnumerator PlayOpeningSequence()
    {
        openingSequenceStarted = true;
        openingMovementLock.Acquire();

        // Cover the scene immediately. Waiting a frame before starting the
        // cutscene lets the newly loaded scene briefly render first.
        if (startCutscenePlayer == null)
        {
            GameObject playerObject = GameObject.Find("StartCutscenePlayer");
            startCutscenePlayer =
                playerObject != null
                    ? playerObject.GetComponent<CutscenePlayer>()
                    : null;
        }

        bool cutsceneFinished = startCutscenePlayer == null;
        startCutscenePlayer?.Play(() => cutsceneFinished = true);

        // Let the persistent player finish its own Start initialization while
        // the cutscene canvas is already covering the gameplay scene.
        yield return null;

        PlayerTransformation transformation = PlayerTransformation.Instance;
        transformation?.ForceWolfForm();
        transformation?.LockTransformation();

        transformation?.HoldWolfRestPose();

        yield return new WaitUntil(() => cutsceneFinished);

        bool dialogueFinished = DialogueManager.Instance == null;
        DialogueManager.Instance?.StartDialogue(
            wakingDialogueID,
            () => dialogueFinished = true);
        yield return new WaitUntil(() => dialogueFinished);

        transformation?.ReleaseWolfRestPose();

        yield return new WaitForSeconds(wakingAnimationDuration);

        ObjectivesUI.Instance?.SetObjective(
            "tutorial.finding_your_footing",
            "explore_trail",
            0);

        openingMovementLock.Release();
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
