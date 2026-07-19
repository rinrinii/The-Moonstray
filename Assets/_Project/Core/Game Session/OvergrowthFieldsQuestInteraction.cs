using UnityEngine;

public class OvergrowthFieldsQuestInteraction : MonoBehaviour,
    IInteractionResponse
{
    public enum Step
    {
        CropOne,
        CropTwo
    }

    private const string QuestID =
        "chapter1.for_every_garden_buries_a_secret";
    private Step step;
    private bool configured;

    public void Configure(
        Step configuredStep)
    {
        step = configuredStep;
        configured = true;

        GameProgressionManager progression = GameProgressionManager.Instance;
        bool completed = progression != null &&
            (step == Step.CropOne
                ? progression.HasFlag(
                    GameProgressionFlags.Chapter1OvergrowthCropOneInspected)
                : progression.HasFlag(
                    GameProgressionFlags.Chapter1OvergrowthCropTwoInspected));
        if (completed)
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
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
                GetComponent<ObjectStateHighlightMarker>()?.Hide();
                SetObjective("inspect_second_rotting_crop", 0);
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
                GetComponent<ObjectStateHighlightMarker>()?.Hide();
                SetObjective("find_harvest_steward", 0);
            });
    }

    private static void SetObjective(string objectiveID, int currentAmount)
    {
        ObjectivesUI.Instance?.SetObjective(
            QuestID, objectiveID, currentAmount);
    }
}
