using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public event Action<TutorialState> OnStateChanged;

    [Header("Tutorial")]
    [SerializeField]
    private TutorialState currentState = TutorialState.None;

    public TutorialState CurrentState => currentState;

    public bool IsTutorialActive =>
        currentState != TutorialState.None &&
        currentState != TutorialState.TutorialComplete;

    public bool IsTutorialFinished =>
        currentState == TutorialState.TutorialComplete;

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

    public void StartTutorial()
    {
        SetState(TutorialState.PinewatchTrail);
    }

    public void SetState(TutorialState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        Debug.Log($"Tutorial State = {currentState}");

        OnStateChanged?.Invoke(currentState);
    }

    public bool IsCurrentState(TutorialState state)
    {
        return currentState == state;
    }

    public bool IsPastState(TutorialState state)
    {
        return currentState >= state;
    }

    public void FinishTutorial()
    {
        SetState(TutorialState.TutorialComplete);
    }
}