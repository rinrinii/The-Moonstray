using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    private IInteractionResponse[] responses;

    [SerializeField] private bool isInteractable = true;

    [Header("Form Requirement")]
    [SerializeField]
    private InteractionFormRequirement requiredForm =
    InteractionFormRequirement.HumanOnly;

    public InteractionFormRequirement RequiredForm => requiredForm;

    [Header("Interaction Animation")]
    public InteractionType interactionType =
        InteractionType.Stand;

    private void Awake()
    {
        responses = GetComponents<IInteractionResponse>();
        Debug.Log($"{name}: Found {responses.Length} interaction responses.");
    }

    public void Interact()
    {
        Debug.Log("Interact called!");

        if (!isInteractable)
            return;

        // Runtime progression bootstraps can add responses after this object's
        // Awake has run, so refresh the cache at the moment of interaction.
        responses = GetComponents<IInteractionResponse>();

        foreach (var response in responses)
        {
            Debug.Log($"Executing {response.GetType().Name}");
            response.OnInteract();
        }
    }
}
