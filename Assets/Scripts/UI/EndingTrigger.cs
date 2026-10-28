using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

[RequireComponent(typeof(BoxCollider))]
public class EndingTrigger : MonoBehaviour
{
    private const string FinalPreludeDialogueID =
        "final.moonveilReflection.prelude";
    private const string FinalProtectBranchDialogueID =
        "final.moonveilReflection.protectBranch";
    private const string FinalHarmBranchDialogueID =
        "final.moonveilReflection.harmBranch";
    private const string FinalContinuationDialogueID =
        "final.moonveilReflection.continuation";
    private const string FinalRestoreHopeBranchDialogueID =
        "final.moonveilReflection.restoreHopeBranch";
    private const string FinalRestoreDoubtBranchDialogueID =
        "final.moonveilReflection.restoreDoubtBranch";
    private const string FinalRestoreConvergenceDialogueID =
        "final.moonveilReflection.restoreConvergence";
    private const string FinalDestroyFreedomBranchDialogueID =
        "final.moonveilReflection.destroyFreedomBranch";
    private const string FinalDestroyRiskBranchDialogueID =
        "final.moonveilReflection.destroyRiskBranch";
    private const string FinalDestroyConvergenceDialogueID =
        "final.moonveilReflection.destroyConvergence";
    private const string FinalQuestTitle = "For Tomorrow Is Ours to Choose";

    [SerializeField]
    private VideoClip goodEndingClip;

    [SerializeField]
    private VideoClip badEndingClip;

    [SerializeField]
    private CutscenePlayer endingCutscenePlayer;

    [Header("Credits")]
    [SerializeField]
    private AudioClip creditsMusic;

    [SerializeField]
    [Min(5f)]
    private float creditsScrollDuration = 45f;

    [SerializeField]
    [Min(0f)]
    private float creditsEndHoldDuration = 3f;

    private bool triggered;
    private bool creditsPlaying;
    private PlayerMovement playerMovement;
    private AudioSource creditsAudioSource;
    private VisualElement creditsPanel;
    private ScrollView creditsScroll;
    private Button creditsCloseButton;
    private Coroutine creditsRoutine;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            progression.CurrentStage != GameProgressionStage.FinalSequence)
        {
            Debug.Log(
                "Ending choice ignored: progression has not reached the final sequence.");
            return;
        }

        triggered = true;

        GameplayUIManager ui = GameplayUIManager.Instance;
        if (ui == null)
            return;

        if (ui.RestoreButton != null)
        {
            ui.RestoreButton.clicked -= PlayGoodEnding;
            ui.RestoreButton.clicked += PlayGoodEnding;
        }

        if (ui.DestroyButton != null)
        {
            ui.DestroyButton.clicked -= PlayBadEnding;
            ui.DestroyButton.clicked += PlayBadEnding;
        }
        playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.StopImmediately();
            playerMovement.enabled = false;
        }

        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue != null)
        {
            dialogue.StartDialogue(
                FinalPreludeDialogueID,
                ShowReflectionChoice);
        }
        else
        {
            Debug.LogWarning(
                "Final reflection dialogue could not start because DialogueManager is missing.");
            ShowFinalDecision();
        }
    }

    private void ShowReflectionChoice()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.ShowPrompt(
            "...",
            "Serin",
            "serin-current",
            new[]
            {
                new DialogueChoice(
                    "They were trying to protect what they loved.",
                    () => PlayReflectionBranch(
                        FinalProtectBranchDialogueID)),
                new DialogueChoice(
                    "That doesn't erase the harm they caused.",
                    () => PlayReflectionBranch(
                        FinalHarmBranchDialogueID))
            });
    }

    private void PlayReflectionBranch(string dialogueID)
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            dialogueID,
            PlayReflectionContinuation);
    }

    private void PlayReflectionContinuation()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            FinalContinuationDialogueID,
            ShowRestoreReflectionChoice);
    }

    private void ShowRestoreReflectionChoice()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.ShowPrompt(
            "...",
            "Serin",
            "serin-current",
            new[]
            {
                new DialogueChoice(
                    "I might be able to bring back the balance that was lost.",
                    () => PlayRestoreReflectionBranch(
                        FinalRestoreHopeBranchDialogueID)),
                new DialogueChoice(
                    "I might only rebuild the same dependence.",
                    () => PlayRestoreReflectionBranch(
                        FinalRestoreDoubtBranchDialogueID))
            });
    }

    private void PlayRestoreReflectionBranch(string dialogueID)
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            dialogueID,
            PlayRestoreConvergence);
    }

    private void PlayRestoreConvergence()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            FinalRestoreConvergenceDialogueID,
            ShowDestroyReflectionChoice);
    }

    private void ShowDestroyReflectionChoice()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.ShowPrompt(
            "...",
            "Serin",
            "serin-current",
            new[]
            {
                new DialogueChoice(
                    "The world would have to find its own way forward.",
                    () => PlayDestroyReflectionBranch(
                        FinalDestroyFreedomBranchDialogueID)),
                new DialogueChoice(
                    "The world might not be ready to lose them.",
                    () => PlayDestroyReflectionBranch(
                        FinalDestroyRiskBranchDialogueID))
            });
    }

    private void PlayDestroyReflectionBranch(string dialogueID)
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            dialogueID,
            PlayDestroyConvergence);
    }

    private void PlayDestroyConvergence()
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            ShowFinalDecision();
            return;
        }

        dialogue.StartDialogue(
            FinalDestroyConvergenceDialogueID,
            ShowFinalDecision);
    }

    private void ShowFinalDecision()
    {
        QuestManager.Instance?.SetObjective(
            FinalQuestTitle,
            "Choose the artifact's fate.");
        GameplayUIManager.Instance?.ShowEndingChoice();
    }

    private void OnDisable()
    {
        GameplayUIManager ui = GameplayUIManager.Instance;
        if (ui == null)
            return;

        if (ui.RestoreButton != null)
            ui.RestoreButton.clicked -= PlayGoodEnding;
        if (ui.DestroyButton != null)
            ui.DestroyButton.clicked -= PlayBadEnding;

        if (creditsCloseButton != null)
            creditsCloseButton.clicked -= FinishCredits;
    }

    private void PlayGoodEnding()
    {
        PlayEnding(goodEndingClip);
    }

    private void PlayBadEnding()
    {
        PlayEnding(badEndingClip);
    }

    private void PlayEnding(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Ending video clip is missing.");
            return;
        }

        GameplayUIManager.Instance?.HideEndingChoice();
        GameplayUIManager.Instance?.SetGameplayUIVisible(false);

        if (endingCutscenePlayer == null)
        {
            endingCutscenePlayer = FindFirstObjectByType<CutscenePlayer>(
                FindObjectsInactive.Include);
        }

        if (endingCutscenePlayer == null)
        {
            Debug.LogWarning("Ending CutscenePlayer is missing.");
            return;
        }

        if (ScreenFade.Instance != null)
        {
            ScreenFade.Instance.FadeOut(
                () => StartEndingCutscene(clip));
        }
        else
        {
            StartEndingCutscene(clip);
        }
    }

    private void StartEndingCutscene(VideoClip clip)
    {
        endingCutscenePlayer.Play(clip, HandleEndingFinished);
        ScreenFade.Instance?.FadeIn();
    }

    private void HandleEndingFinished()
    {
        GameProgressionManager.Instance?.CompleteGame();

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(StartCredits);
        else
            StartCredits();
    }

    private void StartCredits()
    {
        GameplayUIManager ui = GameplayUIManager.Instance;
        if (ui?.RootVisualElement == null)
        {
            Debug.LogWarning("Credits could not open because the gameplay UI is missing.");
            ReturnToMainMenu();
            return;
        }

        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UI/CreditsTemplate");
        if (template == null)
        {
            Debug.LogWarning("Credits template is missing from Resources/UI.");
            ReturnToMainMenu();
            return;
        }

        // The ending cutscene hides the gameplay UI root. A visible child
        // cannot override display:none on its parent, so restore the root
        // before adding the full-screen credits overlay.
        ui.SetGameplayUIVisible(true);
        template.CloneTree(ui.RootVisualElement);
        creditsPanel = ui.RootVisualElement.Q<VisualElement>("CreditsRoot");
        creditsScroll = creditsPanel?.Q<ScrollView>("CreditsScroll");
        creditsCloseButton = creditsPanel?.Q<Button>("CreditsCloseButton");
        PopulateSpecialThanks(creditsPanel);

        if (creditsPanel == null || creditsScroll == null)
        {
            Debug.LogWarning("Credits template is missing its required elements.");
            ReturnToMainMenu();
            return;
        }

        creditsPanel.style.display = DisplayStyle.Flex;
        creditsPanel.BringToFront();
        creditsScroll.scrollOffset = Vector2.zero;

        // The ending sequence owns the scroll position. Ignore pointer,
        // wheel, scrollbar-drag, and keyboard-focus input while preserving
        // programmatic updates from AutoScrollCredits.
        creditsScroll.pickingMode = PickingMode.Ignore;
        creditsScroll.focusable = false;
        creditsScroll.verticalScroller.pickingMode = PickingMode.Ignore;

        // Ending credits are intentionally non-skippable. The same template
        // keeps its close button when opened normally from the main menu.
        if (creditsCloseButton != null)
            creditsCloseButton.style.display = DisplayStyle.None;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        creditsPlaying = true;

        MusicManager.Instance?.FadeOutForCutscene(0.5f);
        PlayCreditsMusic();

        creditsRoutine = StartCoroutine(AutoScrollCredits());
        ScreenFade.Instance?.FadeIn();
    }

    private void PlayCreditsMusic()
    {
        if (creditsMusic == null)
        {
            Debug.LogWarning("Credits music is missing.");
            return;
        }

        creditsAudioSource = gameObject.AddComponent<AudioSource>();
        creditsAudioSource.clip = creditsMusic;
        creditsAudioSource.loop = false;
        creditsAudioSource.playOnAwake = false;
        creditsAudioSource.spatialBlend = 0f;
        creditsAudioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        creditsAudioSource.Play();
    }

    private IEnumerator AutoScrollCredits()
    {
        // UI Toolkit needs a frame to calculate the cloned content's height.
        yield return null;
        yield return null;

        float duration = creditsMusic != null
            ? Mathf.Max(creditsScrollDuration, creditsMusic.length - creditsEndHoldDuration)
            : creditsScrollDuration;
        float elapsed = 0f;

        while (creditsPlaying && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float maximum = Mathf.Max(0f, creditsScroll.verticalScroller.highValue);
            creditsScroll.scrollOffset = new Vector2(
                0f,
                Mathf.Lerp(0f, maximum, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        float holdElapsed = 0f;
        while (creditsPlaying && holdElapsed < creditsEndHoldDuration)
        {
            holdElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (creditsPlaying)
            FinishCredits();
    }

    private void FinishCredits()
    {
        if (!creditsPlaying)
            return;

        creditsPlaying = false;

        if (creditsRoutine != null)
        {
            StopCoroutine(creditsRoutine);
            creditsRoutine = null;
        }

        if (creditsAudioSource != null)
            creditsAudioSource.Stop();

        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(LoadMainMenu);
        else
            LoadMainMenu();
    }

    private static void LoadMainMenu()
    {
        SceneLoader.LoadScene("MainMenu");
    }

    private static void PopulateSpecialThanks(VisualElement root)
    {
        VisualElement container = root?.Q<VisualElement>("SpecialThanksNames");
        TextAsset creditsData = Resources.Load<TextAsset>("Data/Credits");
        if (container == null || creditsData == null)
            return;

        container.Clear();

        try
        {
            XmlDocument document = new();
            document.LoadXml(creditsData.text);
            XmlNodeList people = document.SelectNodes("/credits/specialThanks/person");
            if (people == null)
                return;

            foreach (XmlNode person in people)
            {
                string personName = person.Attributes?["name"]?.Value;
                if (string.IsNullOrWhiteSpace(personName))
                    continue;

                Label nameLabel = new(personName);
                nameLabel.AddToClassList("credits-special-thanks-name");
                container.Add(nameLabel);
            }
        }
        catch (XmlException exception)
        {
            Debug.LogWarning($"Credits XML could not be read: {exception.Message}");
        }
    }
}
