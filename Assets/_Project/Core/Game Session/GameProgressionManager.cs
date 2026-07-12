using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgressionManager : MonoBehaviour
{
    public static GameProgressionManager Instance { get; private set; }

    public event Action<GameProgressionStage> OnStageChanged;
    public event Action<string, bool> OnFlagChanged;

    [SerializeField]
    private GameProgressionStage currentStage = GameProgressionStage.Tutorial;

    private readonly HashSet<string> progressionFlags = new();
    private static bool isCreatingFallback;
    private bool isFallbackInstance;

    public GameProgressionStage CurrentStage => currentStage;
    public bool HasStartedMainJourney => currentStage != GameProgressionStage.Tutorial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameProgressionManager existingManager =
            FindFirstObjectByType<GameProgressionManager>(
                FindObjectsInactive.Include);

        if (existingManager != null)
            return;

        isCreatingFallback = true;
        GameObject managerObject = new GameObject(nameof(GameProgressionManager));
        managerObject.AddComponent<GameProgressionManager>();
        isCreatingFallback = false;
        DontDestroyOnLoad(managerObject);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance.isFallbackInstance)
            {
                Destroy(Instance.gameObject);
                Instance = this;
                PreserveIfNeeded();
                return;
            }

            Destroy(gameObject);
            return;
        }

        isFallbackInstance = isCreatingFallback;
        Instance = this;
        PreserveIfNeeded();
    }

    private void PreserveIfNeeded()
    {
        if (GetComponentInParent<PersistentRoot>() != null)
            return;

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
        RefreshProgressionBlockers();
    }

    public bool IsAtLeast(GameProgressionStage stage)
    {
        return currentStage >= stage;
    }

    public bool HasFlag(string flag)
    {
        return string.IsNullOrWhiteSpace(flag) ||
            progressionFlags.Contains(flag);
    }

    public void SetFlag(string flag, bool value = true)
    {
        if (string.IsNullOrWhiteSpace(flag))
            return;

        bool changed = value
            ? progressionFlags.Add(flag)
            : progressionFlags.Remove(flag);

        if (!changed)
            return;

        Debug.Log($"Game Progression Flag '{flag}' = {value}");
        OnFlagChanged?.Invoke(flag, value);
        RefreshProgressionBlockers();
    }

    private void RefreshProgressionBlockers()
    {
        ProgressionExitBlocker[] blockers =
            FindObjectsByType<ProgressionExitBlocker>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

        foreach (ProgressionExitBlocker blocker in blockers)
            blocker.RefreshState();
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
