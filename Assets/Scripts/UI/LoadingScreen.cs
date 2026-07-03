using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneToLoad;

    [Header("UI")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Settings")]
    [SerializeField] private float minimumLoadTime = 1.5f;
    [SerializeField] private float fadeDuration = 0.75f;

    private void Start()
    {
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        fadePanel.alpha = 1f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minimumLoadTime);

            float progress = Mathf.Min(loadProgress, timeProgress);

            loadingBar.value = progress;

            if (loadProgress >= 1f && elapsed >= minimumLoadTime)
                break;

            yield return null;
        }

        loadingBar.value = 1f;

        yield return FadeOut();

        operation.allowSceneActivation = true;
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 0f;
    }
}