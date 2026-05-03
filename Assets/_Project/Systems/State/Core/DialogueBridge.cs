using UnityEngine;

/// <summary>
/// DialogueBridge acts as a temporary connector between the state system
/// and the actual dialogue system.
///
/// Right now it only logs dialogue IDs.
/// Later, this will call the real DialogueManager.
/// </summary>
public static class DialogueBridge
{
    public static void Show(string dialogueID)
    {
        // temporary: debug output for testing
        Debug.Log("[DIALOGUE] " + dialogueID);

        /*
         * for future integration:
         * replace the line above with actual dialogue system call.
         *
         * ex. (depending on their implementation):
         *
         * DialogueManager.Instance.StartDialogue(dialogueID);
         *
         * OR:
         * DialogueSystem.Play(dialogueID);
         *
         * IMPORTANT:
         * - dialogueID should match entries in their dialogue database
         * - DO NOT change how this method is called elsewhere
         */
    }
}