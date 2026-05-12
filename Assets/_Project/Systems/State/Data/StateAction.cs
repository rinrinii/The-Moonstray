using UnityEngine;

[System.Serializable]
public class StateAction
{
    [Tooltip("The current state required to trigger this action")]
    public int state;

    [Header("Dialogue")]
    [Tooltip("ID used by the Dialogue System to fetch dialogue")]
    public string dialogueID;

    [Header("State Transition")]
    [Tooltip("State to move to after this action is executed")]
    public int nextState;

    public void Execute(string objectID)
    {
        // Check current state
        int currentState = GameStateManager.Instance.GetState(objectID);

        if (currentState != state)
        {
            Debug.Log($"[STATE] Condition not met for {objectID}");
            return;
        }

        // Trigger dialogue if exists
        if (!string.IsNullOrEmpty(dialogueID))
        {
            // DialogueManager.Instance.StartDialogue(dialogueID);
            Debug.Log($"Trigger dialogue: {dialogueID}");
        }

        //  Move to next state
        GameStateManager.Instance.SetState(objectID, nextState);
    }
}