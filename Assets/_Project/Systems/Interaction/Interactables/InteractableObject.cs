using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    private IInteractionResponse[] responses;

    [SerializeField] private bool isInteractable = true;

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

        foreach (var response in responses)
        {
            Debug.Log($"Executing {response.GetType().Name}");
            response.OnInteract();
        }
    }
}