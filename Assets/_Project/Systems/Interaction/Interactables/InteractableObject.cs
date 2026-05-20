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
    }

    public void Interact()
    {
        if (!isInteractable) return;

        foreach (var response in responses)
        {
            response.OnInteract();
        }
    }
}