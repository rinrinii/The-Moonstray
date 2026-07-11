using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance { get; private set; }

    public event Action<GameProgressionStage> OnStageChanged;

    [SerializeField]
    private GameProgressionStage currentStage = GameProgressionStage.Tutorial;

    public GameProgressionStage CurrentStage => currentStage;
    public bool HasStartedMainJourney => currentStage != GameProgressionStage.Tutorial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject managerObject = new GameObject(nameof(GameProgressionManager));
        managerObject.AddComponent<GameProgressionManager>();
        DontDestroyOnLoad(managerObject);
    }

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
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCompleteTutorialAtMoonveil(scene.name);
    }

    public void CompleteTutorialAndBeginChapterOne()
    {
        if (currentStage != GameProgressionStage.Tutorial)
            return;

        ObjectivesUI.Instance?.Clear();
        TutorialManager.Instance?.FinishTutorial();
        RestorePlayerForNewChapter();
        SetStage(GameProgressionStage.Chapter1Spring);
    }

    public void BeginChapterTwo()
    {
        SetStage(GameProgressionStage.Chapter2Summer);
    }

    public void BeginChapterThree()
    {
        SetStage(GameProgressionStage.Chapter3Autumn);
    }

    public void BeginChapterFour()
    {
        SetStage(GameProgressionStage.Chapter4Winter);
    }

    public void BeginFinalSequence()
    {
        SetStage(GameProgressionStage.FinalSequence);
    }

    public void CompleteGame()
    {
        SetStage(GameProgressionStage.Complete);
    }

    public void SetStage(GameProgressionStage stage)
    {
        if (currentStage == stage)
            return;

        currentStage = stage;

        Debug.Log($"Game Progression Stage = {currentStage}");
        OnStageChanged?.Invoke(currentStage);
    }

    private void TryCompleteTutorialAtMoonveil(string sceneName)
    {
        if (sceneName != "Moonveil")
            return;

        if (TutorialManager.Instance == null ||
            TutorialManager.Instance.CurrentState != TutorialState.ReadingWing)
        {
            return;
        }

        CompleteTutorialAndBeginChapterOne();
    }

    private void RestorePlayerForNewChapter()
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        playerHealth?.RestoreFullHealth();

        StatusEffectManager.Instance?.ClearAll();
        PlayerTransformation.Instance?.SetSpeedModifier(1f);
    }
}
