using System.Collections.Generic;
using UnityEngine;

public class DialogueStageInteraction : MonoBehaviour, IInteractionResponse
{
    [SerializeField]
    private List<DialogueStage> stages = new();

    [SerializeField]
    private int currentStage = 0;

    [SerializeField]
    private bool facePlayerOnInteract = true;

    [SerializeField]
    private bool freezePlayerDuringDialogue = true;

    public int CurrentStage => currentStage;

    public void OnInteract()
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning($"{name}: No dialogue stages assigned.");
            return;
        }

        if (currentStage < 0 || currentStage >= stages.Count)
        {
            Debug.LogWarning($"{name}: Invalid dialogue stage ({currentStage}).");
            return;
        }

        DialogueStage stage = stages[currentStage];

        if (string.IsNullOrEmpty(stage.dialogueID))
        {
            Debug.LogWarning($"{name}: Dialogue ID is empty.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("DialogueManager missing.");
            return;
        }

        if (DialogueManager.Instance.IsDialogueActive)
            return;

        FacePlayer();
        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();
        bool wasPlayerMovementEnabled =
            playerMovement != null && playerMovement.enabled;

        if (freezePlayerDuringDialogue && playerMovement != null)
            playerMovement.enabled = false;

        DialogueManager.Instance.StartDialogue(
            stage.dialogueID,
            () =>
            {
                if (stage.advanceAfterDialogue)
                {
                    SetStage(stage.nextStage);
                }

                if (freezePlayerDuringDialogue &&
                    wasPlayerMovementEnabled &&
                    playerMovement != null)
                {
                    playerMovement.enabled = true;
                }
            });

        if (freezePlayerDuringDialogue &&
            wasPlayerMovementEnabled &&
            playerMovement != null &&
            !DialogueManager.Instance.IsDialogueActive)
        {
            playerMovement.enabled = true;
        }
    }

    private void FacePlayer()
    {
        if (!facePlayerOnInteract)
            return;

        PlayerMovement playerMovement =
            FindFirstObjectByType<PlayerMovement>();

        if (playerMovement == null)
            return;

        NPCMovement npcMovement =
            GetComponentInParent<NPCMovement>();

        if (npcMovement == null)
            return;

        npcMovement.FaceTarget(playerMovement.transform);
    }

    public void SetStage(int stage)
    {
        if (stage < 0 || stage >= stages.Count)
        {
            Debug.LogWarning($"{name}: Tried to set invalid dialogue stage ({stage}).");
            return;
        }

        currentStage = stage;
    }

    public void SetStage(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            Debug.LogWarning($"{name}: Stage name is empty.");
            return;
        }

        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].stageName == stageName)
            {
                currentStage = i;
                return;
            }
        }

        Debug.LogWarning($"{name}: Dialogue stage '{stageName}' not found.");
    }

    public void NextStage()
    {
        SetStage(currentStage + 1);
    }

    public void PreviousStage()
    {
        SetStage(currentStage - 1);
    }

}
