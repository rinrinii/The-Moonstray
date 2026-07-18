using System;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
[RequireComponent(typeof(RestoreBehaviour))]
public class RestorationPuzzleInteraction : MonoBehaviour,
    IInteractionResponse
{
    [Serializable]
    private struct MaterialRequirement
    {
        public ItemData item;
        [Min(1)] public int amount;
    }

    [Header("Restoration")]
    [SerializeField] private RestoreBehaviour restoreBehaviour;
    [SerializeField] private Texture2D puzzleImage;
    [SerializeField] private MaterialRequirement[] requiredMaterials;

    [Header("Dialogue IDs")]
    [SerializeField] private string inspectionDialogueID;
    [SerializeField] private string missingMaterialsDialogueID;
    [SerializeField] private string restoredDialogueID;

    [Header("Progression Flags")]
    [Tooltip("Unique flag set after the first inspection. Leave empty to skip inspection gating.")]
    [SerializeField] private string inspectedFlag;
    [Tooltip("Unique flag set after restoration and used to restore the model after loading.")]
    [SerializeField] private string restoredFlag;

    [Header("Objective (Optional)")]
    [SerializeField] private string objectiveTitle;
    [TextArea]
    [SerializeField] private string gatherMaterialsObjective;

    private bool restoring;

    private void Start()
    {
        restoreBehaviour ??= GetComponent<RestoreBehaviour>();
        restoreBehaviour?.PrepareRuntimeReplacement();

        if (IsRestored())
        {
            restoreBehaviour?.Execute();
            return;
        }

        if (!NeedsInspection())
            ShowGatherMaterialsObjective();
    }

    public void OnInteract()
    {
        if (restoring || IsRestored() ||
            DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (NeedsInspection())
        {
            StartDialogue(inspectionDialogueID, CompleteInspection);
            return;
        }

        if (!HasRequiredMaterials())
        {
            StartDialogue(missingMaterialsDialogueID);
            return;
        }

        RestorationPuzzleUI puzzle =
            GameplayUIManager.Instance?.RestorationPuzzle;

        if (puzzle == null || puzzleImage == null)
        {
            Debug.LogWarning(
                $"{name}: Puzzle UI or puzzle image missing; restoring immediately.");
            CompleteRestoration();
            return;
        }

        restoring = true;
        puzzle.Open(
            puzzleImage,
            CompleteRestoration,
            () => restoring = false);
    }

    private bool NeedsInspection()
    {
        return !string.IsNullOrWhiteSpace(inspectedFlag) &&
            (GameProgressionManager.Instance == null ||
             !GameProgressionManager.Instance.HasFlag(inspectedFlag));
    }

    private bool IsRestored()
    {
        return !string.IsNullOrWhiteSpace(restoredFlag) &&
            GameProgressionManager.Instance != null &&
            GameProgressionManager.Instance.HasFlag(restoredFlag);
    }

    private void CompleteInspection()
    {
        GameProgressionManager.Instance?.SetFlag(inspectedFlag);

        ShowGatherMaterialsObjective();
    }

    private void ShowGatherMaterialsObjective()
    {
        if (!string.IsNullOrWhiteSpace(objectiveTitle) &&
            !string.IsNullOrWhiteSpace(gatherMaterialsObjective))
        {
            ObjectivesUI.Instance?.SetObjective(
                objectiveTitle,
                gatherMaterialsObjective);
        }
    }

    private bool HasRequiredMaterials()
    {
        if (requiredMaterials == null)
            return true;

        foreach (MaterialRequirement requirement in requiredMaterials)
        {
            if (requirement.item == null || requirement.amount <= 0)
                continue;

            if (GetItemAmount(requirement.item) < requirement.amount)
                return false;
        }

        return true;
    }

    private void ConsumeRequiredMaterials()
    {
        if (requiredMaterials == null)
            return;

        foreach (MaterialRequirement requirement in requiredMaterials)
        {
            if (requirement.item != null && requirement.amount > 0)
            {
                InventorySystem.Instance?.Remove(
                    requirement.item,
                    requirement.amount);
            }
        }
    }

    private void CompleteRestoration()
    {
        restoring = true;
        ConsumeRequiredMaterials();

        void SwapModel()
        {
            restoreBehaviour?.Execute();

            void Finish()
            {
                StartDialogue(restoredDialogueID, () =>
                {
                    GameProgressionManager.Instance?.SetFlag(restoredFlag);
                    restoring = false;
                });
            }

            if (ScreenFade.Instance != null)
                ScreenFade.Instance.FadeIn(Finish);
            else
                Finish();
        }

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(SwapModel);
        else
            SwapModel();
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

    private static void StartDialogue(
        string dialogueID,
        Action onComplete = null)
    {
        if (string.IsNullOrWhiteSpace(dialogueID) ||
            DialogueManager.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueID, onComplete);
    }
}
