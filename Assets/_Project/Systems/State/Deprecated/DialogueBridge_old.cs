using UnityEngine;

public static class DialogueBridge
{
    public static void Show(string dialogueID)
    {
        DialogueManager.Instance.StartDialogue(dialogueID);
    }
}