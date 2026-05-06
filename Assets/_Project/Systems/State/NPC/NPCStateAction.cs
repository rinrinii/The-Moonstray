using UnityEngine;

[System.Serializable]
public class NPCStateAction
{
    [Tooltip("Required state to trigger this action")]
    public int requiredState;

    [Tooltip("Dialogue ID to play")]
    public string dialogueID;

    [Tooltip("Next state after interaction")]
    public int nextState;

    public void Execute(string objectID)
    {
        int currentState = GameStateManager.Instance.GetState(objectID);

        if (currentState != requiredState)
            return;

        // Dialogue
        if (!string.IsNullOrEmpty(dialogueID))
        {
            // DialogueManager.Instance.StartDialogue(dialogueID);
            Debug.Log($"Trigger dialogue: {dialogueID}");
        }

        // Progression
        GameStateManager.Instance.SetState(objectID, nextState);
    }
}