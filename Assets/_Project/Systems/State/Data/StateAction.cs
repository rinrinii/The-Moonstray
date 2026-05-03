using UnityEngine;

/// <summary>
/// Defines what happens at a specific state.
/// This is configured in the Unity Inspector.
/// </summary>
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

    /*
     * FUTURE EXPANSION
     * this can be expanded with:
     * - required flags (e.g., needs item)
     * - rewards
     * - animations
     */
}