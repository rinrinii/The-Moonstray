using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public event Action<TutorialStep> OnStepChanged;

    [Header("Tutorial State")]
    [SerializeField]
    private TutorialStep currentStep = TutorialStep.None;

    private bool startTutorialOnNextPinewatchLoad;

    public TutorialStep CurrentStep => currentStep;

    public bool IsTutorialFinished =>
        currentStep == TutorialStep.Finished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called by the New Game button.
    /// The tutorial will begin after Pinewatch Trail finishes loading.
    /// </summary>
    public void QueueTutorialStart()
    {
        startTutorialOnNextPinewatchLoad = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!startTutorialOnNextPinewatchLoad)
            return;

        if (scene.name != "Pinewatch Trail")
            return;

        startTutorialOnNextPinewatchLoad = false;

        StartTutorial();
    }

    public void StartTutorial()
    {
        SetStep(TutorialStep.Move);
    }

    public void SetStep(TutorialStep step)
    {
        if (currentStep == step)
            return;

        currentStep = step;

        Debug.Log($"Tutorial Step: {currentStep}");

        OnStepChanged?.Invoke(currentStep);
    }

    public void FinishTutorial()
    {
        SetStep(TutorialStep.Finished);
    }

    public bool IsCurrentStep(TutorialStep step)
    {
        return currentStep == step;
    }
}