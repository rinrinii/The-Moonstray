using System;
using UnityEngine;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    [SerializeField] private Canvas cutsceneCanvas;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool keepCanvasActiveOnAwake;

    [SerializeField]
    private float musicFadeDuration = 0.75f;

    private Action onFinished;
    private bool isPlaying;
    private bool skipPromptVisible;
    private int skipPromptShownFrame;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private CutsceneSkipPromptView skipPromptView;

    private void Awake()
    {
        if (cutsceneCanvas != null && !keepCanvasActiveOnAwake)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }

    }

    public void Play(Action finishedCallback)
    {
        if (isPlaying)
            return;

        isPlaying = true;
        onFinished = finishedCallback;

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(true);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeOutForCutscene(
                musicFadeDuration);
        }

        videoPlayer.Stop();

        videoPlayer.prepareCompleted -= HandlePrepared;
        videoPlayer.loopPointReached -= HandleVideoFinished;

        videoPlayer.prepareCompleted += HandlePrepared;
        videoPlayer.loopPointReached += HandleVideoFinished;

        videoPlayer.Prepare();
    }

    public void Play(VideoClip clip, Action finishedCallback)
    {
        if (clip == null)
        {
            Debug.LogWarning("CutscenePlayer: Video clip is missing.");
            return;
        }

        videoPlayer.clip = clip;
        Play(finishedCallback);
    }

    /// <summary>
    /// Dismisses the cutscene presentation without invoking its completion
    /// callback. Used when loading a save at a checkpoint after the cutscene.
    /// </summary>
    public void StopAndHide()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= HandlePrepared;
            videoPlayer.loopPointReached -= HandleVideoFinished;
            videoPlayer.Stop();
        }

        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);

        if (isPlaying && MusicManager.Instance != null)
            MusicManager.Instance.FadeInAfterCutscene(musicFadeDuration);

        HideSkipPrompt();
        isPlaying = false;
        onFinished = null;
    }

    private void HandlePrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= HandlePrepared;

        vp.Play();
    }

    private void HandleVideoFinished(VideoPlayer player)
    {
        videoPlayer.loopPointReached -= HandleVideoFinished;

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        videoPlayer.prepareCompleted -= HandlePrepared;
        videoPlayer.loopPointReached -= HandleVideoFinished;

        videoPlayer.Stop();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.FadeInAfterCutscene(
                musicFadeDuration);
        }

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }

        HideSkipPrompt();
        isPlaying = false;

        Action finishedCallback = onFinished;
        onFinished = null;
        finishedCallback?.Invoke();
    }

    private void Update()
    {
        if (isPlaying && !skipPromptVisible &&
            videoPlayer != null && videoPlayer.isPlaying &&
            Input.GetMouseButtonDown(0))
        {
            ShowSkipPrompt();
        }

        if (skipPromptVisible &&
            Time.frameCount > skipPromptShownFrame + 1 &&
            !Input.GetMouseButton(0))
        {
            skipPromptView?.SetButtonsEnabled(true);
        }

        // Debug shortcut
        if (!isPlaying && Input.GetKeyDown(KeyCode.P))
        {
            Play(null);
        }
    }

    private void ShowSkipPrompt()
    {
        skipPromptView ??= CutsceneSkipPromptView.Create(transform);
        if (skipPromptView == null)
            return;

        skipPromptVisible = true;
        skipPromptShownFrame = Time.frameCount;
        videoPlayer.Pause();
        skipPromptView.Show(FinishCutscene, ResumeCutscene);
        skipPromptView.SetButtonsEnabled(false);

        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideSkipPrompt()
    {
        if (!skipPromptVisible)
            return;

        skipPromptVisible = false;
        skipPromptView?.Hide();

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    private void ResumeCutscene()
    {
        HideSkipPrompt();
        videoPlayer.Play();
    }

    private void OnDestroy()
    {
        HideSkipPrompt();
        skipPromptView?.Dispose();
    }
}
