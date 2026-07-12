using UnityEngine;

public class DialogueInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string dialogueID;

    [SerializeField] private bool facePlayerOnInteract = true;

    public void OnInteract()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning("Dialogue ID is empty.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        FacePlayer();
        DialogueManager.Instance.StartDialogue(dialogueID);
    }

    private void FacePlayer()
    {
        if (!facePlayerOnInteract)
            return;

        NPCMovement npcMovement =
            GetComponentInParent<NPCMovement>();

        if (npcMovement == null)
            return;

        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            npcMovement.FaceTarget(playerMovement.transform);
    }
}
