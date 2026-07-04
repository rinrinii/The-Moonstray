using System;
using UnityEngine;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    [SerializeField] private Canvas cutsceneCanvas;
    [SerializeField] private VideoPlayer videoPlayer;

    [SerializeField]
    private float musicFadeDuration = 0.75f;

    private Action onFinished;
    private bool isPlaying;

    private void Awake()
    {
        if (cutsceneCanvas != null)
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

    private void HandlePrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= HandlePrepared;

        vp.Play();
    }

    private void HandleVideoFinished(VideoPlayer player)
    {
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

        isPlaying = false;

        onFinished?.Invoke();
        onFinished = null;
    }

    private void Update()
    {
        // Debug shortcut
        if (Input.GetKeyDown(KeyCode.P))
        {
            Play(null);
        }
    }
}