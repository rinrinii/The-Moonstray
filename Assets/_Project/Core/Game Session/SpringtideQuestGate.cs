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
            DialogueManager.Instance.StartDialogue(dialogueID);
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueID, () =>
        {
            progression.SetFlag(completionFlag);
            ObjectivesUI.Instance?.SetObjective(
                "For Every Garden Buries a Secret",
                nextObjective);
        });
    }
}
