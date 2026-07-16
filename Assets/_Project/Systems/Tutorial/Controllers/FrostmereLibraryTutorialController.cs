using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class FrostmereLibraryTutorialController : MonoBehaviour
{
    private bool wakeInitialized;
    private bool searchInitialized;
    private bool revealInitialized;
    private bool readingInitialized;

    private readonly PlayerMovementFreezeHandle readingSequenceMovementLock =
        new();

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
    private string restDialogueID = "intro.serinWake";

    [SerializeField]
    private float restPromptDelay = 2f;

    [SerializeField]
    private CollectBehaviour requiredArchiveObject;

    [SerializeField]
    private Transform npcPlayerDestination;

    [SerializeField]
    private GameObject restrictedArchivesExitBlocker;

    private const int requiredArchivesProgress = 3;

    private int archivesProgress;
    private bool archiveObjectCollected;
    private int notesRead;

    private bool hasTransformedToHuman;

    private bool npcReturning;
    private bool awaitingRestInput;
    private bool restSequenceStarted;

    #endregion

    #region Reveal Identity

    [Header("Reveal Identity")]

    [SerializeField]
    private string revealDialogueID = "intro.studentReturn";

    [SerializeField]
    private Transform readingWingPlayerSpawn;

    [SerializeField]
    private Transform readingWingNpcSpawn;

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

    [SerializeField]
    private Transform firstBookshelfPoint;

    [SerializeField]
    private Transform secondBookshelfPoint;

    [SerializeField]
    private CutscenePlayer readingWingCutscene;
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

        CollectBehaviour.OnItemCollected -= HandleItemCollected;
        NoteInteractionResponse.OnNoteRead -= HandleNoteRead;

        if (playerTransformation != null)
            playerTransformation.OnTransformationComplete -= HandleTransformationCompleted;

        readingSequenceMovementLock.Release();
    }

    private void Update()
    {
        if (awaitingRestInput && Input.GetKeyDown(KeyCode.F))
        {
            BeginRestSequence();
            return;
        }

        if (!searchInitialized)
            return;

        if (archiveObjectCollected && notesRead == 0 &&
            Input.GetKeyDown(KeyCode.I))
        {
            PromptUI.Instance?.Hide();
        }
        else if (notesRead == 1 && Input.GetKeyDown(KeyCode.J))
        {
            PromptUI.Instance?.Hide();
        }
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

    private void RegisterSearchEvents()
    {
        CollectBehaviour.OnItemCollected -= HandleItemCollected;
        NoteInteractionResponse.OnNoteRead -= HandleNoteRead;

        if (playerTransformation != null)
            playerTransformation.OnTransformationComplete -= HandleTransformationCompleted;

        CollectBehaviour.OnItemCollected += HandleItemCollected;
        NoteInteractionResponse.OnNoteRead += HandleNoteRead;

        if (playerTransformation != null)
            playerTransformation.OnTransformationComplete += HandleTransformationCompleted;
    }

    private void WalkNpcTo(Transform destination, Action onArrived)
    {
        if (tutorialNpc == null || destination == null)
        {
            onArrived?.Invoke();
            return;
        }

        tutorialNpc.WalkTo(destination, onArrived);
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

        GameplayUIManager.Instance.Inventory?.Unlock();
        GameplayUIManager.Instance.Journal?.Unlock();

        StatusEffectManager.Instance?.ClearAll();

        // Restore vignette

        // Keep HP at 10%

        // Stand player up
    }

    private void DisablePlayerMovement()
    {
        ResolvePlayerReferences();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        ResolvePlayerReferences();

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void ResolvePlayerReferences()
    {
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerTransformation == null)
            playerTransformation = FindFirstObjectByType<PlayerTransformation>();
    }

    private void OnWakeDialogueFinished()
    {
        if (tutorialNpc != null)
        {
            WalkNpcTo(
                npcSuppliesDestination,
                OnNpcReachedSupplies);
        }
    }

    private void OnNpcReachedSupplies()
    {
        TutorialManager.Instance.SetState(
            TutorialState.SearchArchives);
    }

    #endregion

    #region Search Archives

    private void EnterSearchArchives()
    {
        if (searchInitialized)
            return;

        hasTransformedToHuman = false;
        searchInitialized = true;

        archiveObjectCollected = false;
        notesRead = 0;
        archivesProgress = 0;
        npcReturning = false;
        awaitingRestInput = false;
        restSequenceStarted = false;

        RegisterSearchEvents();

        SetRestrictedArchivesExitBlockerActive(true);
        EnableSearchHUD();
        DisablePlayerMovement();
        playerTransformation?.LockTransformation();
        PromptUI.Instance?.Hide();

        StartCoroutine(ShowRestPromptAfterDelay());
    }

    private IEnumerator ShowRestPromptAfterDelay()
    {
        yield return new WaitForSeconds(restPromptDelay);

        ObjectivesUI.Instance?.SetObjective(
            "Rest",
            "Rest and gather your strength.");

        PromptUI.Instance?.Show(
            "[F] Rest",
            "Press F to rest.");

        awaitingRestInput = true;
        EnablePlayerMovement();
    }

    private void BeginRestSequence()
    {
        if (restSequenceStarted)
            return;

        awaitingRestInput = false;
        restSequenceStarted = true;

        PromptUI.Instance?.Hide();
        DisablePlayerMovement();

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(OnRestFadeOutFinished);
        else
            OnRestFadeOutFinished();
    }

    private void OnRestFadeOutFinished()
    {
        ResolvePlayerReferences();
        playerTransformation?.ForceHumanForm();

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeIn(OnRestFadeInFinished);
        else
            OnRestFadeInFinished();
    }

    private void OnRestFadeInFinished()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(
                restDialogueID,
                OnRestDialogueFinished);
        else
            OnRestDialogueFinished();
    }

    private void OnRestDialogueFinished()
    {
        UpdateSearchObjective();
        EnableTransformation();
        EnablePlayerMovement();

        PromptUI.Instance?.Show(
            "[E] Interact",
            "Press E to interact.");
    }

    private void UpdateSearchObjective()
    {
        ObjectivesUI.Instance?.SetObjective(
            "Echoes of the Past",
            $"Explore the archives ({archivesProgress}/{requiredArchivesProgress})");
    }

    private void EnableSearchHUD()
    {
        if (HUDController.Instance == null)
            return;

        HUDController.Instance.SetObjectivesVisible(true);
        HUDController.Instance.SetBottomRightHUDVisible(true);
        SetMapShortcutVisible(false);
    }

    private void SetMapShortcutVisible(bool visible)
    {
        VisualElement mapShortcut =
            GameplayUIManager.Instance?.RootVisualElement?.Q<VisualElement>(
                "MinimapIcon");

        if (mapShortcut != null)
        {
            mapShortcut.style.display =
                visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void EnableTransformation()
    {
        playerTransformation?.UnlockTransformation();
    }

    private void CheckSearchArchivesCompleted()
    {
        if (!archiveObjectCollected)
            return;

        if (notesRead < 2)
            return;

        if (!hasTransformedToHuman)
        {
            ObjectivesUI.Instance?.SetObjective(
                "Echoes of the Past",
                "Transform into your human form.");

            return;
        }

        if (npcReturning)
            return;

        npcReturning = true;
        LockPlayerForReadingSequence();
        PromptUI.Instance?.Hide();

        ObjectivesUI.Instance?.SetObjective(
            "Echoes of the Past",
            "Wait for the student.");

        CollectBehaviour.OnItemCollected -= HandleItemCollected;
        NoteInteractionResponse.OnNoteRead -= HandleNoteRead;

        if (playerTransformation != null)
            playerTransformation.OnTransformationComplete -= HandleTransformationCompleted;

        ReturnNpc();
    }

    private void ReturnNpc()
    {
        if (tutorialNpc == null)
        {
            OnNpcReturned();
            return;
        }

        WalkNpcTo(
            npcPlayerDestination,
            OnNpcReturned);
    }

    private void OnNpcReturned()
    {
        SetRestrictedArchivesExitBlockerActive(false);
        FaceCharacters();

        TutorialManager.Instance.SetState(
            TutorialState.RevealIdentity);
    }

    private void SetRestrictedArchivesExitBlockerActive(bool active)
    {
        if (restrictedArchivesExitBlocker == null)
        {
            restrictedArchivesExitBlocker =
                GameObject.Find("RestrictedArchivesExitBlocker");
        }

        if (restrictedArchivesExitBlocker != null)
            restrictedArchivesExitBlocker.SetActive(active);
    }

    private void HandleTransformationCompleted(
    PlayerTransformation.FormState form)
    {
        if (form != PlayerTransformation.FormState.Human)
            return;

        if (hasTransformedToHuman)
            return;

        hasTransformedToHuman = true;

        CheckSearchArchivesCompleted();
    }

    private void HandleItemCollected(CollectBehaviour collectedObject)
    {
        if (collectedObject != requiredArchiveObject)
            return;

        if (archiveObjectCollected)
            return;

        archiveObjectCollected = true;
        archivesProgress++;

        UpdateSearchObjective();

        if (notesRead == 0)
        {
            PromptUI.Instance?.Show(
                "[I] Inventory",
                "Press I to open your inventory.");
        }

        CheckSearchArchivesCompleted();
    }

    private void HandleNoteRead()
    {
        if (notesRead >= 2)
            return;

        notesRead++;
        archivesProgress++;

        UpdateSearchObjective();

        if (notesRead == 1)
        {
            PromptUI.Instance?.Show(
                "[J] Journal",
                "Press J to open your journal.");
        }
        else
        {
            PromptUI.Instance?.Hide();
        }

        CheckSearchArchivesCompleted();
    }

    #endregion

    #region Reveal Identity

    private void EnterRevealIdentity()
    {
        if (revealInitialized)
            return;

        revealInitialized = true;
        LockPlayerForReadingSequence();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                revealDialogueID,
                OnRevealDialogueFinished);
        }
    }

    private void OnRevealDialogueFinished()
    {
        DisablePlayerMovement();

        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeOut(OnRevealFadeOutFinished);
        }
        else
        {
            OnRevealFadeOutFinished();
        }
    }

    private void OnRevealFadeOutFinished()
    {
        RestorePlayerHealth();

        MovePlayerToReadingWing();
        MoveNpcToReadingWing();

        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeIn(OnRevealFadeInFinished);
        }
        else
        {
            OnRevealFadeInFinished();
        }

        FaceCharacters();
    }

    private void OnRevealFadeInFinished()
    {
        TutorialManager.Instance.SetState(
            TutorialState.ReadingWing);
    }

    private void RestorePlayerHealth()
    {
        ResolvePlayerReferences();
        playerHealth?.RestoreFullHealth();
    }

    private void MovePlayerToReadingWing()
    {
        ResolvePlayerReferences();

        if (playerMovement == null || readingWingPlayerSpawn == null)
        {
            Debug.LogError(
                "Cannot move player to Reading Wing: player or spawn reference is missing.");
            return;
        }

        Transform player = playerMovement.transform;
        CharacterController characterController =
            player.GetComponent<CharacterController>();

        if (characterController != null)
            characterController.enabled = false;

        player.SetPositionAndRotation(
            readingWingPlayerSpawn.position,
            readingWingPlayerSpawn.rotation);

        Physics.SyncTransforms();

        if (characterController != null)
            characterController.enabled = true;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement?.ResetVerticalVelocity();

        FallDamage fallDamage = player.GetComponent<FallDamage>();
        fallDamage?.ResetFallTracking();

        Debug.Log($"Moved player to Reading Wing spawn at {player.position}.");
    }

    private void MoveNpcToReadingWing()
    {
        if (tutorialNpc == null || readingWingNpcSpawn == null)
            return;

        tutorialNpc.transform.SetPositionAndRotation(
            readingWingNpcSpawn.position,
            readingWingNpcSpawn.rotation);
    }

    private void FaceCharacters()
    {
        if (playerMovement == null || tutorialNpc == null)
            return;

        Vector3 playerLook =
            tutorialNpc.transform.position -
            playerMovement.transform.position;

        playerLook.y = 0f;

        playerMovement.transform.rotation =
            Quaternion.LookRotation(playerLook);

        Vector3 npcLook =
            playerMovement.transform.position -
            tutorialNpc.transform.position;

        npcLook.y = 0f;

        tutorialNpc.transform.rotation =
            Quaternion.LookRotation(npcLook);
    }

    #endregion

    #region Reading Wing

    private void EnterReadingWing()
    {
        if (readingInitialized)
            return;

        readingInitialized = true;
        LockPlayerForReadingSequence();

        StartReadingDialogue1();
    }

    private void LockPlayerForReadingSequence()
    {
        readingSequenceMovementLock.Acquire();
    }

    private void StartReadingDialogue1()
    {
        DialogueManager.Instance?.StartDialogue(
            readingDialogue1ID,
            OnReadingDialogue1Finished);
    }

    private void OnReadingDialogue1Finished()
    {
        WalkNpcTo(
            firstBookshelfPoint,
            OnReachedFirstBookshelf);
    }

    private void OnReachedFirstBookshelf()
    {
        DialogueManager.Instance?.StartDialogue(
            readingDialogue2ID,
            OnReadingDialogue2Finished);
    }

    private void OnReadingDialogue2Finished()
    {
        WalkNpcTo(
            secondBookshelfPoint,
            OnReachedSecondBookshelf);
    }

    private void OnReachedSecondBookshelf()
    {
        DialogueManager.Instance?.StartDialogue(
            readingDialogue3ID,
            OnReadingDialogue3Finished);
    }

    private void OnReadingDialogue3Finished()
    {
        DisablePlayerMovement();

        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeOut(OnReadingFadeOutFinished);
        }
        else
        {
            OnReadingFadeOutFinished();
        }
    }

    private void OnReadingFadeOutFinished()
    {
        if (readingWingCutscene != null)
        {
            readingWingCutscene.Play(OnReadingCutsceneFinished);
        }
        else
        {
            OnReadingCutsceneFinished();
        }
    }

    private void OnReadingCutsceneFinished()
    {
        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeIn(OnReadingFadeInFinished);
        }
        else
        {
            OnReadingFadeInFinished();
        }
    }

    private void OnReadingFadeInFinished()
    {
        DialogueManager.Instance?.StartDialogue(
            giveMapDialogueID,
            OnGiveMapDialogueFinished);
    }

    private void OnGiveMapDialogueFinished()
    {
        GameplayUIManager.Instance.Map?.Unlock();
        SetMapShortcutVisible(true);

        DialogueManager.Instance?.StartDialogue(
            farewellDialogueID,
            OnFarewellDialogueFinished);
    }

    private void OnFarewellDialogueFinished()
    {
        readingSequenceMovementLock.Release();

        ObjectivesUI.Instance?.SetObjective(
            "Leaving the Past Behind",
            "Leave the Frostmere Library.");

        Debug.Log("Reading Wing sequence complete. Tutorial remains active until Moonveil.");
    }


    #endregion
}
