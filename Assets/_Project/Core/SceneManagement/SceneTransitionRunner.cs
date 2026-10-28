using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneTransitionRunner : MonoBehaviour
{
    private static SceneTransitionRunner instance;
    private bool isTransitioning;

    public static void Begin(string sceneName)
    {
        EnsureInstance();
        instance?.BeginTransition(sceneName);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject host = PersistentRoot.Instance != null
            ? PersistentRoot.Instance.gameObject
            : new GameObject(nameof(SceneTransitionRunner));

        instance = host.GetComponent<SceneTransitionRunner>();
        if (instance == null)
            instance = host.AddComponent<SceneTransitionRunner>();

        if (PersistentRoot.Instance == null)
            DontDestroyOnLoad(host);
    }

    private void BeginTransition(string sceneName)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(sceneName))
            return;

        isTransitioning = true;

        void LoadBehindFade()
        {
            StartCoroutine(LoadSceneAndReveal(sceneName));
        }

        ScreenFade fade = ScreenFade.Instance;
        if (fade != null && !fade.IsFullyBlack)
            fade.FadeOut(LoadBehindFade);
        else
            LoadBehindFade();
    }

    private IEnumerator LoadSceneAndReveal(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            isTransitioning = false;
            yield break;
        }

        while (!operation.isDone)
            yield return null;

        // SceneSpawnManager and save restoration place the player during the
        // first initialized frame. Keep the screen black until those moves
        // have completed, then snap the persistent camera before revealing it.
        yield return null;
        yield return new WaitForEndOfFrame();

        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
        cameraFollow?.SnapToTarget();

        SceneLoader.PendingScene = null;
        isTransitioning = false;

        ScreenFade.Instance?.FadeIn();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
