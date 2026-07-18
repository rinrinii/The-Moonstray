using UnityEngine;

public class OvergrowthFieldsQuestInteraction : MonoBehaviour,
    IInteractionResponse
{
    public enum Step
    {
        CropOne,
        CropTwo
    }

    private const string QuestTitle = "For Every Garden Buries a Secret";
    private Step step;
    private bool configured;

    public void Configure(
        Step configuredStep)
    {
        step = configuredStep;
        configured = true;
    }

    public void OnInteract()
    {
        if (!configured ||
            DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null)
            return;

        switch (step)
        {
            case Step.CropOne:
                InspectCropOne(progression);
                break;
            case Step.CropTwo:
                InspectCropTwo(progression);
                break;
        }
    }

    private void InspectCropOne(GameProgressionManager progression)
    {
        if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
        {
            DialogueManager.Instance.StartDialogue(
                "chapter1.inspectOvergrowthCropOne");
            return;
        }

        DialogueManager.Instance.StartDialogue(
            "chapter1.inspectOvergrowthCropOne",
            () =>
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1OvergrowthCropOneInspected);
                SetObjective("Inspect the rotting crops. (1/2)");
            });
    }

    private void InspectCropTwo(GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
        {
            return;
        }

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropTwoInspected))
        {
            DialogueManager.Instance.StartDialogue(
                "chapter1.inspectOvergrowthCropTwo");
            return;
        }

        DialogueManager.Instance.StartDialogue(
            "chapter1.inspectOvergrowthCropTwo",
            () =>
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1OvergrowthCropTwoInspected);
                SetObjective(
                    "Look for the Harvest Steward of Springtide Meadows.");
            });
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}
