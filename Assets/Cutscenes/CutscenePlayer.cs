using System;
using UnityEngine;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    [SerializeField] private Canvas cutsceneCanvas;
    [SerializeField] private VideoPlayer videoPlayer;

    private Action onFinished;

    private void Awake()
    {
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }
    }

    public void Play(Action finishedCallback)
    {
        onFinished = finishedCallback;

        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(true);

        videoPlayer.loopPointReached += HandleVideoFinished;

        videoPlayer.prepareCompleted += HandlePrepared;

        videoPlayer.Prepare();
    }

    private void HandlePrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= HandlePrepared;

        Debug.Log($"Prepared!");
        Debug.Log($"Width: {vp.texture?.width}");
        Debug.Log($"Height: {vp.texture?.height}");
        Debug.Log($"Texture: {vp.texture}");

        vp.Play();
    }

    private void HandleVideoFinished(VideoPlayer player)
    {
        videoPlayer.loopPointReached -= HandleVideoFinished;

        videoPlayer.Stop();

        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.gameObject.SetActive(false);
        }

        onFinished?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Play(null);
        }
    }
}