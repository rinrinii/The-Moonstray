using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

[RequireComponent(typeof(BoxCollider))]
public class EndingTrigger : MonoBehaviour
{
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
        ui.ShowEndingChoice();

        playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;
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

        endingCutscenePlayer.Play(clip, HandleEndingFinished);
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
        // Load the camera-backed loading scene before scheduling destruction
        // of the persistent gameplay camera. This prevents a frame where
        // Unity has no enabled camera while returning to the title screen.
        SceneLoader.LoadScene("MainMenu");
        PersistentRoot.DestroyPersistentSystems();
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
