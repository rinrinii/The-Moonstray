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

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueID);
    }
}