using System.Collections;
using UnityEngine;

public class EastWingController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    private GameObject waningRoot;

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
    private float waningDisappearDelay = 1f;

    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError(
                "EastWingController: PlayerHealth not found."
            );

            enabled = false;
            return;
        }

        playerHealth.OnStoryDeath +=
            HandleStoryDeath;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnStoryDeath -=
                HandleStoryDeath;
        }
    }

    private void HandleStoryDeath()
    {
        Debug.Log("EastWingController: Story death received.");

        playerHealth.OnStoryDeath -= HandleStoryDeath;

        StartCoroutine(StorySequence());
    }

    private IEnumerator StorySequence()
    {
        Debug.Log("Story sequence started.");

        yield return new WaitForSeconds(waningDisappearDelay);

        Debug.Log("Attempting to start dialogue...");

        if (waningRoot != null)
        {
            waningRoot.SetActive(false);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                dialogueID
            );

            yield return new WaitUntil(() =>
                !DialogueManager.Instance.IsDialogueActive());
        }

        SceneLoader.LoadScene(
            nextScene,
            spawnID
        );
    }
}