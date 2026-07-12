using UnityEngine;

public class OvergrowthFieldsQuestInteraction : MonoBehaviour,
    IInteractionResponse
{
    public enum Step
    {
        CropOne,
        CropTwo,
        RuinedGarden
    }

    private const string QuestTitle = "For Every Garden Buries a Secret";
    private const int RequiredMaterialAmount = 3;

    private Step step;
    private RestoreBehaviour restoreBehaviour;
    private ItemData lumberBundle;
    private ItemData stonePile;
    private bool configured;
    private bool restoring;

    public void Configure(
        Step configuredStep,
        RestoreBehaviour configuredRestore,
        ItemData configuredLumberBundle,
        ItemData configuredStonePile)
    {
        step = configuredStep;
        restoreBehaviour = configuredRestore;
        lumberBundle = configuredLumberBundle;
        stonePile = configuredStonePile;
        configured = true;

        if (step == Step.RuinedGarden &&
            GameProgressionManager.Instance != null &&
            GameProgressionManager.Instance.HasFlag(
                GameProgressionFlags.Chapter1RuinedGardenRestored))
        {
            restoreBehaviour?.Execute();
        }
    }

    public void OnInteract()
    {
        if (!configured || restoring ||
            DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            !progression.HasFlag(
                GameProgressionFlags.Chapter1VillageIrrigationRestored))
        {
            return;
        }

        switch (step)
        {
            case Step.CropOne:
                InspectCropOne(progression);
                break;
            case Step.CropTwo:
                InspectCropTwo(progression);
                break;
            case Step.RuinedGarden:
                InteractWithGarden(progression);
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

    private void InteractWithGarden(GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropTwoInspected) ||
            progression.HasFlag(
                GameProgressionFlags.Chapter1RuinedGardenRestored))
        {
            return;
        }

        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1RuinedGardenInspected))
        {
            DialogueManager.Instance.StartDialogue(
                "chapter1.inspectRuinedGarden",
                () => progression.SetFlag(
                    GameProgressionFlags.Chapter1RuinedGardenInspected));
            return;
        }

        if (!HasMaterials())
        {
            DialogueManager.Instance.StartDialogue(
                "chapter1.ruinedGardenMissingMaterials");
            return;
        }

        RestoreGarden(progression);
    }

    private void RestoreGarden(GameProgressionManager progression)
    {
        restoring = true;
        ConsumeMaterials();

        void SwapGarden()
        {
            restoreBehaviour?.Execute();

            void RevealGarden()
            {
                DialogueManager.Instance?.StartDialogue(
                    "chapter1.restoreRuinedGarden",
                    () =>
                    {
                        progression.SetFlag(
                            GameProgressionFlags.Chapter1RuinedGardenRestored);
                        restoring = false;
                    });
            }

            if (ScreenFade.Instance != null)
                ScreenFade.Instance.FadeIn(RevealGarden);
            else
                RevealGarden();
        }

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(SwapGarden);
        else
            SwapGarden();
    }

    private bool HasMaterials()
    {
        return GetItemAmount(lumberBundle) >= RequiredMaterialAmount &&
            GetItemAmount(stonePile) >= RequiredMaterialAmount;
    }

    private void ConsumeMaterials()
    {
        InventorySystem.Instance?.Remove(
            lumberBundle,
            RequiredMaterialAmount);
        InventorySystem.Instance?.Remove(
            stonePile,
            RequiredMaterialAmount);
    }

    private static int GetItemAmount(ItemData item)
    {
        if (item == null || InventorySystem.Instance == null)
            return 0;

        int amount = 0;
        foreach (InventorySystem.Slot slot in InventorySystem.Instance.slots)
        {
            if (slot.item == item ||
                (slot.item != null && slot.item.itemID == item.itemID))
            {
                amount += slot.amount;
            }
        }

        return amount;
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}
