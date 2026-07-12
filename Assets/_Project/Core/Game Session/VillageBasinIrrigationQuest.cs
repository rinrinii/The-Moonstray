using UnityEngine;

public class VillageBasinIrrigationQuest : MonoBehaviour, IInteractionResponse
{
    private const string QuestTitle = "For Every Garden Buries a Secret";

    private RestoreBehaviour restoreBehaviour;
    private ItemData shovel;
    private bool configured;
    private bool restoring;

    public void Configure(
        RestoreBehaviour configuredRestoreBehaviour,
        ItemData configuredShovel)
    {
        restoreBehaviour = configuredRestoreBehaviour;
        shovel = configuredShovel;
        configured = true;

        InventorySystem inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= HandleInventoryChanged;
            inventory.OnInventoryChanged += HandleInventoryChanged;
        }

        RefreshState();
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= HandleInventoryChanged;
    }

    public void OnInteract()
    {
        if (!configured || restoring)
            return;

        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            !progression.HasFlag(GameProgressionFlags.Chapter1GreenhouseInspected) ||
            progression.HasFlag(
                GameProgressionFlags.Chapter1VillageIrrigationRestored) ||
            DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1VillageIrrigationInspected))
        {
            InspectIrrigation(progression);
            return;
        }

        if (!HasShovel())
        {
            SetObjective("Obtain a shovel.");
            return;
        }

        RestoreIrrigation(progression);
    }

    private void InspectIrrigation(GameProgressionManager progression)
    {
        DialogueManager.Instance.StartDialogue(
            "chapter1.inspectVillageIrrigation",
            () =>
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1VillageIrrigationInspected);
                UpdateShovelProgress(progression);
            });
    }

    private void RestoreIrrigation(GameProgressionManager progression)
    {
        restoring = true;

        void SwapModel()
        {
            restoreBehaviour.Execute();

            void RevealRestoration()
            {
                DialogueManager.Instance?.StartDialogue(
                    "chapter1.restoreVillageIrrigation",
                    () =>
                    {
                        progression.SetFlag(
                            GameProgressionFlags.Chapter1VillageIrrigationRestored);
                        SetObjective("Investigate more signs of the Waning.");
                        restoring = false;
                    });
            }

            if (ScreenFade.Instance != null)
                ScreenFade.Instance.FadeIn(RevealRestoration);
            else
                RevealRestoration();
        }

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(SwapModel);
        else
            SwapModel();
    }

    private void HandleInventoryChanged()
    {
        if (!configured)
            return;

        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            !progression.HasFlag(
                GameProgressionFlags.Chapter1VillageIrrigationInspected))
        {
            return;
        }

        UpdateShovelProgress(progression);
    }

    private void UpdateShovelProgress(GameProgressionManager progression)
    {
        if (HasShovel())
        {
            progression.SetFlag(GameProgressionFlags.Chapter1ShovelObtained);
            SetObjective("Restore the irrigation mechanism.");
        }
        else
        {
            SetObjective("Obtain a shovel.");
        }
    }

    private void RefreshState()
    {
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null)
            return;

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1VillageIrrigationRestored))
        {
            restoreBehaviour.Execute();
            SetObjective("Investigate more signs of the Waning.");
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1VillageIrrigationInspected))
        {
            UpdateShovelProgress(progression);
        }
        else
        {
            SetObjective("Inspect the old irrigation system.");
        }
    }

    private bool HasShovel()
    {
        if (shovel == null || InventorySystem.Instance == null)
            return false;

        foreach (InventorySystem.Slot slot in InventorySystem.Instance.slots)
        {
            if (slot.item == shovel ||
                (slot.item != null && slot.item.itemID == shovel.itemID))
            {
                return slot.amount > 0;
            }
        }

        return false;
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}
