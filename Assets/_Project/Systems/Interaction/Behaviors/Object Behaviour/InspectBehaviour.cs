using UnityEngine;

public class InspectBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueID;

    private bool hasBeenInspected;

    public void Execute()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning(
                $"InspectBehaviour on '{gameObject.name}' has no Dialogue ID."
            );
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        DialogueManager.Instance.StartDialogue(
            dialogueID,
            () =>
            {
                Debug.Log($"Inspection complete: {gameObject.name}");

                if (!hasBeenInspected)
                {
                    hasBeenInspected = true;

                    SnowCourtyardTutorialController.Instance?.RegisterInspection();
                }

            // Future:
            // - Notify tutorial controller
            // - Trigger quest progression
            // - Play SFX
            });
    }
}