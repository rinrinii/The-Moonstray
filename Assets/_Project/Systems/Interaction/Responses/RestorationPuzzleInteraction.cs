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
    [SerializeField] private string puzzleTitle = "Restore the Ruined Garden";
    [SerializeField] private string puzzleInstructions =
        "Rotate each fragment to reconstruct the garden.";
    [SerializeField] private MaterialRequirement[] requiredMaterials;

    [Header("Dialogue IDs")]
    [SerializeField] private string inspectionDialogueID;
    [SerializeField] private string missingMaterialsDialogueID;
    [SerializeField] private string restoredDialogueID;

    [Header("Progression Flags")]
    [Tooltip("Optional flag required before this repair interaction becomes available.")]
    [SerializeField] private string requiredProgressionFlag;
    [Tooltip("Unique flag set after the first inspection. Leave empty to skip inspection gating.")]
    [SerializeField] private string inspectedFlag;
    [Tooltip("Unique flag set after restoration and used to restore the model after loading.")]
    [SerializeField] private string restoredFlag;

    [Header("Objective (Optional)")]
    [SerializeField] private string objectiveTitle;
    [TextArea]
    [SerializeField] private string gatherMaterialsObjective;
    [SerializeField] private string objectiveQuestID;
    [SerializeField] private string gatherMaterialsObjectiveID;
    [SerializeField] private string materialsReadyObjectiveID;
    [SerializeField] private string restoredObjectiveID;

    private bool restoring;

    public void ConfigureSingleMaterial(
        RestoreBehaviour configuredRestoreBehaviour,
        Texture2D configuredPuzzleImage,
        ItemData requiredItem,
        int requiredAmount,
        string configuredInspectionDialogueID,
        string configuredMissingMaterialsDialogueID,
        string configuredRestoredDialogueID,
        string configuredInspectedFlag,
        string configuredRestoredFlag,
        string configuredObjectiveQuestID,
        string configuredGatherObjectiveID,
        string configuredMaterialsReadyObjectiveID,
        string configuredRestoredObjectiveID,
        string configuredPuzzleTitle,
        string configuredPuzzleInstructions)
    {
        restoreBehaviour = configuredRestoreBehaviour;
        puzzleImage = configuredPuzzleImage;
        requiredMaterials = requiredItem == null
            ? Array.Empty<MaterialRequirement>()
            : new[]
            {
                new MaterialRequirement
                {
                    item = requiredItem,
                    amount = Mathf.Max(1, requiredAmount)
                }
            };
        inspectionDialogueID = configuredInspectionDialogueID;
        missingMaterialsDialogueID = configuredMissingMaterialsDialogueID;
        restoredDialogueID = configuredRestoredDialogueID;
        inspectedFlag = configuredInspectedFlag;
        restoredFlag = configuredRestoredFlag;
        objectiveQuestID = configuredObjectiveQuestID;
        gatherMaterialsObjectiveID = configuredGatherObjectiveID;
        materialsReadyObjectiveID = configuredMaterialsReadyObjectiveID;
        restoredObjectiveID = configuredRestoredObjectiveID;
        puzzleTitle = configuredPuzzleTitle;
        puzzleInstructions = configuredPuzzleInstructions;

        restoreBehaviour?.PrepareRuntimeReplacement();
        if (IsRestored())
            restoreBehaviour?.Execute();
    }

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
        {
            if (HasRequiredMaterials())
                ShowMaterialsReadyObjective();
            else
                ShowGatherMaterialsObjective();
        }

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged += HandleInventoryChanged;
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= HandleInventoryChanged;
    }

    public void OnInteract()
    {
        if (restoring || IsRestored() ||
            DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredProgressionFlag) &&
            (GameProgressionManager.Instance == null ||
             !GameProgressionManager.Instance.HasFlag(requiredProgressionFlag)))
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

        ShowMaterialsReadyObjective();

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
            () => restoring = false,
            puzzleTitle,
            puzzleInstructions);
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

        if (HasRequiredMaterials())
            ShowMaterialsReadyObjective();
        else
            ShowGatherMaterialsObjective();
    }

    private void ShowGatherMaterialsObjective()
    {
        if (!string.IsNullOrWhiteSpace(objectiveQuestID) &&
            !string.IsNullOrWhiteSpace(gatherMaterialsObjectiveID))
        {
            ObjectivesUI.Instance?.SetObjective(
                objectiveQuestID,
                gatherMaterialsObjectiveID,
                0);
            return;
        }

        if (!string.IsNullOrWhiteSpace(objectiveTitle) &&
            !string.IsNullOrWhiteSpace(gatherMaterialsObjective))
        {
            ObjectivesUI.Instance?.SetObjective(
                objectiveTitle,
                gatherMaterialsObjective);
        }
    }

    private void HandleInventoryChanged()
    {
        if (!NeedsInspection() && !IsRestored() && HasRequiredMaterials())
            ShowMaterialsReadyObjective();
    }

    private void ShowMaterialsReadyObjective()
    {
        if (!string.IsNullOrWhiteSpace(objectiveQuestID) &&
            !string.IsNullOrWhiteSpace(materialsReadyObjectiveID))
        {
            ObjectivesUI.Instance?.SetObjective(
                objectiveQuestID,
                materialsReadyObjectiveID,
                0);
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
                    if (!string.IsNullOrWhiteSpace(objectiveQuestID) &&
                        !string.IsNullOrWhiteSpace(restoredObjectiveID))
                    {
                        ObjectivesUI.Instance?.SetObjective(
                            objectiveQuestID,
                            restoredObjectiveID,
                            0);
                    }
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
