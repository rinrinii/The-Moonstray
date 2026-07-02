using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public event Action<TutorialStep> OnStepChanged;

    private bool startTutorialOnSceneLoad;

    public bool ShouldStartTutorial => startTutorialOnSceneLoad;

    [Header("Tutorial State")]
    [SerializeField]
    private TutorialStep currentStep = TutorialStep.None;

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

    /// <summary>
    /// Called by Main Menu after loading Pinewatch Trail.
    /// </summary>
    public void QueueTutorialStart()
    {
        Debug.Log("Tutorial queued");

        startTutorialOnSceneLoad = true;
    }

    /// <summary>
    /// Called by TutorialBootstrap once Pinewatch Trail finishes loading.
    /// </summary>
    public void StartTutorial()
    {
        Debug.Log("StartTutorial()");
        startTutorialOnSceneLoad = false;

        QuestManager.Instance?.StartQuest(
            "Finding Your Footing",
            new string[]
            {
                "Move",
                "Sprint",
                "Jump",
                "Reach the Courtyard"
            });

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

    public bool IsCurrentStep(TutorialStep step)
    {
        return currentStep == step;
    }

    public void CompleteCurrentStep()
    {
        switch (currentStep)
        {
            case TutorialStep.Move:

                QuestManager.Instance?.CompleteCurrentObjective();
                SetStep(TutorialStep.Sprint);
                break;

            case TutorialStep.Sprint:

                QuestManager.Instance?.CompleteCurrentObjective();
                SetStep(TutorialStep.Jump);
                break;

            case TutorialStep.Jump:

                QuestManager.Instance?.CompleteCurrentObjective();
                SetStep(TutorialStep.ReachCourtyard);
                break;

            case TutorialStep.ReachCourtyard:

                QuestManager.Instance?.CompleteCurrentObjective();

                QuestManager.Instance?.StartQuest(
                    "Gathering Supplies",
                    "Collect Supplies",
                    "Continue to Blight Path"
                );

                SetStep(TutorialStep.CollectSupplies);

                break;

            case TutorialStep.CollectSupplies:

                QuestManager.Instance?.CompleteCurrentObjective();

                SetStep(TutorialStep.ReachBlight);

                break;

            case TutorialStep.ReachBlight:

                QuestManager.Instance?.CompleteCurrentObjective();

                FinishTutorial();

                break;


        }
    }

    public void FinishTutorial()
    {
        SetStep(TutorialStep.Finished);
    }
}