using UnityEngine;

public class SpringtideQuestGate : MonoBehaviour, IInteractionResponse
{
    private string dialogueID;
    private string requiredFlag;
    private string completionFlag;
    private string nextObjective;

    public void Configure(
        string configuredDialogueID,
        string configuredRequiredFlag,
        string configuredCompletionFlag,
        string configuredNextObjective)
    {
        dialogueID = configuredDialogueID;
        requiredFlag = configuredRequiredFlag;
        completionFlag = configuredCompletionFlag;
        nextObjective = configuredNextObjective;

        if (GameProgressionManager.Instance != null &&
            GameProgressionManager.Instance.HasFlag(completionFlag))
        {
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
        }
    }

    public void OnInteract()
    {
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null || !progression.HasFlag(requiredFlag))
            return;

        if (DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
            return;

        if (progression.HasFlag(completionFlag))
        {
            // A repairable quest target (the Outer Farmlands greenhouse)
            // hands subsequent interactions to its restoration puzzle after
            // the story inspection has completed.
            if (GetComponent<RestorationPuzzleInteraction>() == null)
                DialogueManager.Instance.StartDialogue(dialogueID);
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueID, () =>
        {
            progression.SetFlag(completionFlag);
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
            ObjectivesUI.Instance?.SetObjective(
                "chapter1.for_every_garden_buries_a_secret",
                nextObjective,
                0);
        });
    }
}
