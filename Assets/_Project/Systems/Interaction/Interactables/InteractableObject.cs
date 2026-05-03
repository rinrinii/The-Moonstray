using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Interaction Type")]
    public InteractionType interactionType;

    [Header("Basic Interaction")]
    public string message;
    public GameObject targetObject;

    [Header("Visuals")]
    public Color activeColor = Color.green;

    [Header("State System")]
    public bool useStateSystem = false;
    public string objectID;

    public StateAction[] stateActions;

    private Renderer rend;
    private Color originalColor;
    private bool state;

    private ICustomInteractable customLogic;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        customLogic = GetComponent<ICustomInteractable>();
    }

    // MAIN INTERACTION ENTRY
    public void Interact()
    {
        if (useStateSystem)
        {
            if (string.IsNullOrEmpty(objectID))
            {
                Debug.LogWarning("State system enabled but objectID is missing!");
                return;
            }

            HandleStateInteraction();
        }
        else
        {
            HandleBasicInteraction();
        }
    }

    // BASIC (ENUM-DRIVEN)
    void HandleBasicInteraction()
    {
        switch (interactionType)
        {
            case InteractionType.Inspect:
                Debug.Log(message);
                break;

            case InteractionType.Toggle:
                if (targetObject != null)
                {
                    targetObject.SetActive(!targetObject.activeSelf);
                    Debug.Log("Toggled " + targetObject.name);
                }
                break;

            case InteractionType.StateChange:
                state = !state;

                if (rend != null)
                    rend.material.color = state ? activeColor : originalColor;

                Debug.Log("State: " + state);
                break;

            case InteractionType.Custom:
                customLogic?.CustomInteract();
                break;
        }
    }

    // STATE-DRIVEN (DATA-DRIVEN)
    void HandleStateInteraction()
    {
        int currentState = GameStateManager.Instance.GetState(objectID);

        Debug.Log($"[INTERACT] {objectID} state: {currentState}");

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager not found in scene!");
            return;
        }

        foreach (var action in stateActions)
        {
            if (action.state == currentState)
            {
                // DIALOGUE TRIGGER POINT
                if (!string.IsNullOrEmpty(action.dialogueID))
                {
                    /*
                     * This is where dialogue is triggered.
                     *
                     * The system does NOT know how dialogue works.
                     * It only passes a dialogueID.
                     *
                     * DialogueBridge will handle calling the real system.
                     * 
                     * need to implement a DialogueManager (to look up dialogueID through xml loader); format as entity.dialogue
                     * 
                     * example in dialogue manager, using dialogueID.split
                        string[] parts = dialogueID.Split('.');
                        string entityID = parts[0];
                        string dialogueKey = parts[1];
                     * 
                     * example in xml:
                     * <Dialogues>
                            <Entity id="npc01">
                                <Dialogue id="intro">
                                    <Line>Hello traveler...</Line>
                                </Dialogue>

                                <Dialogue id="quest">
                                    <Line>Can you find my item?</Line>
                                </Dialogue>
                            </Entity>
                        </Dialogues>
                     * 
                     * 
                     */
                    DialogueBridge.Show(action.dialogueID);
                }

                // STATE TRANSITION
                GameStateManager.Instance.SetState(objectID, action.nextState);

                return;
            }
        }

        Debug.Log("No state action defined for state: " + currentState);
    }

    // HIGHLIGHT
    public void Highlight()
    {
        if (rend != null)
            rend.material.color = Color.yellow;
    }

    public void Unhighlight()
    {
        if (rend != null)
        {
            if (state)
                rend.material.color = activeColor;
            else
                rend.material.color = originalColor;
        }
    }
}