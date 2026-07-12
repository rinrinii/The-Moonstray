using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProgressionExitBlocker : MonoBehaviour
{
    [SerializeField]
    private GameProgressionStage unlockStage =
        GameProgressionStage.Complete;

    [SerializeField]
    private string requiredFlag;

    [SerializeField]
    private bool permanentlyUnlock = true;

    private bool hasUnlocked;
    private bool isSubscribed;
    private GameProgressionManager progression;

    public void Configure(
        GameProgressionStage stage,
        string flag = "",
        bool unlockPermanently = true)
    {
        unlockStage = stage;
        requiredFlag = flag;
        permanentlyUnlock = unlockPermanently;
        Refresh();
    }

    private void Reset()
    {
        Collider blockerCollider = GetComponent<Collider>();
        blockerCollider.isTrigger = false;
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void Start()
    {
        RefreshState();
    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
            RefreshState();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        progression = GameProgressionManager.Instance;

        if (progression == null)
            return;

        progression.OnStageChanged += HandleStageChanged;
        progression.OnFlagChanged += HandleFlagChanged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed || progression == null)
            return;

        progression.OnStageChanged -= HandleStageChanged;
        progression.OnFlagChanged -= HandleFlagChanged;
        progression = null;
        isSubscribed = false;
    }

    private void HandleStageChanged(GameProgressionStage stage)
    {
        RefreshState();
    }

    private void HandleFlagChanged(string flag, bool value)
    {
        RefreshState();
    }

    public void RefreshState()
    {
        if (hasUnlocked && permanentlyUnlock)
        {
            gameObject.SetActive(false);
            return;
        }

        if (progression == null)
            progression = GameProgressionManager.Instance;

        if (progression == null)
            return;

        bool shouldUnlock =
            progression.IsAtLeast(unlockStage) &&
            progression.HasFlag(requiredFlag);

        if (!shouldUnlock)
            return;

        hasUnlocked = true;
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        RefreshState();
    }
}
