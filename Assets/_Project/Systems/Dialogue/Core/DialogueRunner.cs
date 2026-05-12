using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private string dialogueID;

    public void Run()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueID);
    }
}