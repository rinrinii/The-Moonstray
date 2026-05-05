using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private string dialogueID;

    public void Run()
    {
        DialogueManager.Instance.StartDialogue(dialogueID);
    }
}