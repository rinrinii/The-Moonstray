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

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        // Dialogue + progression
        if (!string.IsNullOrEmpty(dialogueID))
        {
            DialogueManager.Instance.StartDialogue(
                dialogueID,
                () =>
                {
                    GameStateManager.Instance.SetState(
                        objectID,
                        nextState
                    );
                });
        }
        else
        {
            // No dialogue; advance immediately.
            GameStateManager.Instance.SetState(
                objectID,
                nextState
            );
        }
    }
}