using UnityEngine;

public class BlightTutorialController : MonoBehaviour
{
    [Header("Exit Blockers")]
    [SerializeField] private GameObject courtyardExitBlocker;

    public static BlightTutorialController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (TutorialManager.Instance == null)
            return;

        TutorialManager.Instance.OnStateChanged += HandleStateChanged;

        HandleStateChanged(TutorialManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.BlightPath:
                EnterBlightPath();
                break;

            case TutorialState.Collapse:
                ExitBlightPath();
                break;
        }
    }

    private void EnterBlightPath()
    {
        Debug.Log("Entered Blight Path Tutorial");

        if (courtyardExitBlocker != null)
            courtyardExitBlocker.SetActive(true);

        PlayerTransformation.Instance?.ForceWolfForm();
        PlayerTransformation.Instance?.LockTransformation();

        HUDController.Instance?.SetBottomRightHUDVisible(false);

        ObjectivesUI.Instance?.SetObjective(
            "Searching for Clues",
            "Continue following the trail."
        );

        PromptUI.Instance?.Hide();
    }

    private void ExitBlightPath()
    {
        Debug.Log("Leaving Blight Path Tutorial");

        PromptUI.Instance?.Hide();
        ObjectivesUI.Instance?.Clear();
    }
}