using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    [SerializeField] private float minimumLoadTime = 1.5f;
    [SerializeField] private float fillSpeed = 3f;

    private IEnumerator Start()
    {
        if (loadingBar != null)
            loadingBar.value = 0f;

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(SceneLoader.PendingScene);

        operation.allowSceneActivation = false;

        float elapsed = 0f;
        float displayedProgress = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            float actualProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            displayedProgress =
                Mathf.MoveTowards(
                    displayedProgress,
                    actualProgress,
                    fillSpeed * Time.deltaTime
                );

            if (loadingBar != null)
                loadingBar.value = displayedProgress;

            if (actualProgress >= 1f &&
                displayedProgress >= 0.999f &&
                elapsed >= minimumLoadTime)
            {
                break;
            }

            yield return null;
        }

        if (loadingBar != null)
            loadingBar.value = 1f;

        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;
    }
}
