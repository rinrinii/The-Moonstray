using System.Collections;
using UnityEngine;

public class EastWingTutorialController : MonoBehaviour
{
    [Header("Exit Blocker")]
    [SerializeField]
    private GameObject blightExitBlocker;

    [Header("Dialogue")]
    [SerializeField]
    private string dialogueID = "intro.prologue";

    [Header("Scene Transition")]
    [SerializeField]
    private string nextScene = "Frostmere Library";

    [SerializeField]
    private string spawnID = "ToRestrictedArchivesFromEastWing";

    [Header("Timing")]
    [SerializeField]
    private float storyDeathDelay = 1f;

    private PlayerHealth playerHealth;

    public static EastWingTutorialController Instance
    {
        get;
        private set;
    }

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
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(TutorialManager.Instance.CurrentState);
        }

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.OnStoryDeath += HandleStoryDeath;
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStateChanged -= HandleStateChanged;

        if (playerHealth != null)
            playerHealth.OnStoryDeath -= HandleStoryDeath;
    }

    private void HandleStateChanged(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.Collapse:
                EnterCollapse();
                break;

            case TutorialState.WakeInLibrary:
                ExitCollapse();
                break;
        }
    }

    private void EnterCollapse()
    {
        Debug.Log("Entered Collapse Tutorial");

        if (blightExitBlocker != null)
            blightExitBlocker.SetActive(true);

        HUDController.Instance?.SetBottomRightHUDVisible(false);

        ObjectivesUI.Instance?.SetObjective(
            "Searching for Clues",
            "Investigate the area."
        );

        PromptUI.Instance?.Hide();
    }

    private void ExitCollapse()
    {
        PromptUI.Instance?.Hide();
        ObjectivesUI.Instance?.Clear();
    }

    private void HandleStoryDeath()
    {
        playerHealth.OnStoryDeath -= HandleStoryDeath;
        StartCoroutine(StorySequence());
    }

    private IEnumerator StorySequence()
    {
        yield return new WaitForSeconds(storyDeathDelay);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueID);

            yield return new WaitUntil(() =>
                !DialogueManager.Instance.IsDialogueActive());
        }

        playerHealth?.RestoreAfterStoryDeath();

        TutorialManager.Instance?.SetState(TutorialState.WakeInLibrary);

        SceneLoader.LoadScene(nextScene, spawnID);
    }
}