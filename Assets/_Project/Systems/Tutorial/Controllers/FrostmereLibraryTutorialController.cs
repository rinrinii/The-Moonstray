using UnityEngine;

public class FrostmereLibraryTutorialController : MonoBehaviour
{
    private bool wakeInitialized;
    private bool searchInitialized;
    private bool revealInitialized;
    private bool readingInitialized;

    #region Wake In Library

    [Header("Wake In Library")]

    [SerializeField]
    private string wakeDialogueID = "intro.studentWake";

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerTransformation playerTransformation;

    [SerializeField]
    private NPCMovement tutorialNpc;

    [SerializeField]
    private Transform npcSuppliesDestination;

    #endregion

    #region Search Archives

    [Header("Search Archives")]
    [SerializeField]
    private TutorialPromptTrigger transformationTrigger;

    #endregion

    #region Reveal Identity

    [Header("Reveal Identity")]
    [SerializeField]
    private string revealDialogueID = "intro.studentReturn";

    #endregion

    #region Reading Wing

    [Header("Reading Wing")]
    [SerializeField]
    private string readingDialogue1ID = "intro.studentInform1";

    [SerializeField]
    private string readingDialogue2ID = "intro.studentInform2";

    [SerializeField]
    private string readingDialogue3ID = "intro.studentInform3";

    [SerializeField]
    private string giveMapDialogueID = "intro.studentGiveMap";

    [SerializeField]
    private string farewellDialogueID = "intro.studentFarewell";
    #endregion

    private void Start()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerTransformation = FindFirstObjectByType<PlayerTransformation>();

        if (TutorialManager.Instance == null)
            return;

        TutorialManager.Instance.OnStateChanged += HandleTutorialStateChanged;

        HandleTutorialStateChanged(TutorialManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStateChanged -= HandleTutorialStateChanged;
    }

    private void HandleTutorialStateChanged(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.WakeInLibrary:
                EnterWakeInLibrary();
                break;

            case TutorialState.SearchArchives:
                EnterSearchArchives();
                break;

            case TutorialState.RevealIdentity:
                EnterRevealIdentity();
                break;

            case TutorialState.ReadingWing:
                EnterReadingWing();
                break;
        }
    }



    #region Wake In Library

    private void EnterWakeInLibrary()
    {
        if (wakeInitialized)
            return;

        wakeInitialized = true;

        PrepareWakeSequence();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                wakeDialogueID,
                OnWakeDialogueFinished);
        }
    }

    private void PrepareWakeSequence()
    {
        DisablePlayerMovement();

        // Remove Winter Waning

        // Restore vignette

        // Keep HP at 10%

        // Stand player up
    }

    private void DisablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void OnWakeDialogueFinished()
    {
        if (tutorialNpc != null)
        {
            tutorialNpc.WalkTo(
                npcSuppliesDestination,
                OnNpcReachedSupplies);
        }
    }

    private void OnNpcReachedSupplies()
    {
        TutorialManager.Instance.SetState(
            TutorialState.SearchArchives);

        EnablePlayerMovement();
    }

    #endregion

    #region Search Archives

    private void EnterSearchArchives()
    {
        if (searchInitialized)
            return;

        searchInitialized = true;

        ShowSearchObjective();

        EnableSearchHUD();

        EnableTransformation();

        EnableTransformationTutorial();
    }

    private void ShowSearchObjective()
    {
        ObjectivesUI.Instance?.SetObjective(
            "Searching for Answers",
            "Explore the archives.");
    }

    private void EnableSearchHUD()
    {
        if (HUDController.Instance == null)
            return;

        HUDController.Instance.SetObjectivesVisible(true);
        HUDController.Instance.SetTopRightHUDVisible(true);
        HUDController.Instance.SetBottomRightHUDVisible(true);
    }

    private void EnableTransformation()
    {
        playerTransformation?.UnlockTransformation();
    }

    private void EnableTransformationTutorial()
    {
        if (transformationTrigger != null)
        {
            transformationTrigger.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Reveal Identity

    private void EnterRevealIdentity()
    {

    }

    #endregion

    #region Reading Wing

    private void EnterReadingWing()
    {

    }

    #endregion
}