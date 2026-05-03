using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Interaction Type")]
    public InteractionType interactionType;

    [Header("Inspect")]
    public string message;

    [Header("Toggle")]
    public GameObject targetObject;

    [Header("State")]
    public Color activeColor = Color.green;

    private Renderer rend;
    private Color originalColor;
    private bool state;

    // Optional custom logic hook
    private ICustomInteractable customLogic;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;

        customLogic = GetComponent<ICustomInteractable>();
    }

    public void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Inspect:
                HandleInspect();
                break;

            case InteractionType.Toggle:
                HandleToggle();
                break;

            case InteractionType.StateChange:
                HandleStateChange();
                break;

            case InteractionType.Custom:
                customLogic?.CustomInteract();
                break;
        }
    }

    void HandleInspect()
    {
        Debug.Log(message);
    }

    void HandleToggle()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
            Debug.Log("Toggled: " + targetObject.name);
        }
    }

    void HandleStateChange()
    {
        state = !state;

        if (rend != null)
            rend.material.color = state ? activeColor : originalColor;

        Debug.Log("State changed: " + state);
    }

    public void Highlight()
    {
        if (rend != null)
            rend.material.color = Color.yellow;
    }

    public void Unhighlight()
    {
        if (rend != null)
            rend.material.color = state ? activeColor : originalColor;
    }
}