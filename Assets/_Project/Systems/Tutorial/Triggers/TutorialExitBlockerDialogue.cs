using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialExitBlockerDialogue : MonoBehaviour
{
    [SerializeField]
    private string dialogueID = "tutorial.blockedExit";

    private bool playerInside;

    private void Reset()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerInside || !other.CompareTag("Player"))
            return;

        playerInside = true;

        if (DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.StartDialogue(dialogueID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}
