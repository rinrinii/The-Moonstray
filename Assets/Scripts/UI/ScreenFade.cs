using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage == null)
        {
            Debug.LogError("ScreenFade: Fade Image is not assigned.");
            return;
        }

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        SetRaycastBlocking(false);
        fadeImage.gameObject.SetActive(false);
    }

    public void FadeOut(Action onComplete = null)
    {
        StartFade(1f, onComplete);
    }

    public void FadeIn(Action onComplete = null)
    {
        StartFade(0f, onComplete);
    }

    private void StartFade(float targetAlpha, Action onComplete)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeImage.gameObject.SetActive(true);
        SetRaycastBlocking(true);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            Color color = fadeImage.color;
            color.a = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                elapsed / fadeDuration);

            fadeImage.color = color;

            yield return null;
        }

        Color final = fadeImage.color;
        final.a = targetAlpha;
        fadeImage.color = final;

        fadeRoutine = null;

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            SetRaycastBlocking(false);
            fadeImage.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }

    private void SetRaycastBlocking(bool block)
    {
        fadeImage.raycastTarget = block;
    }
}
