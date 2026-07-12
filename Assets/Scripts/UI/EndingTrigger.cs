using UnityEngine;
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

    private bool triggered;
    private PlayerMovement playerMovement;

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
            ScreenFade.Instance.FadeOut(QuitGame);
        else
            QuitGame();
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
