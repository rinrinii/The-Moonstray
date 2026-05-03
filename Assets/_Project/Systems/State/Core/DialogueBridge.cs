using UnityEngine;

public static class DialogueBridge
{
    public static void Show(string dialogueID)
    {
        string[] parts = dialogueID.Split('.');
        if (parts.Length == 2)
        {
            DialogueManager.Instance.StartDialogue(parts[0], parts[1]);
        }
        else
        {
            Debug.LogError($"Dialogue ID {dialogueID} must be format 'Chapter.Part'");
        }
    }
}