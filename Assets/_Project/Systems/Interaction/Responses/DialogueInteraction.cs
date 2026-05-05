using UnityEngine;

public class DialogueInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string dialogueID;

    public void OnInteract()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning("Dialogue ID is empty.");
            return;
        }

        TryStartDialogue();
    }

    private void TryStartDialogue()
    {
        // since dialogue system is not available in this branch
        Debug.Log($"[DialogueInteraction] Trigger: {dialogueID}");
    }
}