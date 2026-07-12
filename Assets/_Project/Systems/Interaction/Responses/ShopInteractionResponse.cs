using UnityEngine;

public class ShopInteractionResponse : MonoBehaviour, IInteractionResponse
{
    [Header("Welcome Gift")]
    [SerializeField]
    private string welcomeDialogueID = "chapter1.shopWelcome";

    [SerializeField]
    private string welcomeProgressionFlag =
        GameProgressionFlags.Chapter1ShopWelcomeComplete;

    [SerializeField]
    private ItemData welcomeGiftItem;

    [SerializeField]
    private int welcomeGiftAmount = 3;

    [Header("Repeat Dialogue")]
    [SerializeField]
    private string[] repeatDialogueIDs =
    {
        "chapter1.shopRepeat1",
        "chapter1.shopRepeat2",
        "chapter1.shopRepeat3"
    };

    private readonly System.Collections.Generic.List<string> repeatDialogueBag = new();

    public void OnInteract()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            return;
        }

        if (ShouldPlayWelcomeDialogue())
        {
            DialogueManager.Instance.StartDialogue(
                welcomeDialogueID,
                () =>
                {
                    GrantWelcomeGift();
                    GameProgressionManager.Instance?.SetFlag(
                        welcomeProgressionFlag);
                    OpenShop();
                });

            return;
        }

        string repeatDialogueID = GetNextRepeatDialogueID();

        if (!string.IsNullOrWhiteSpace(repeatDialogueID) &&
            DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                repeatDialogueID,
                OpenShop);

            return;
        }

        OpenShop();
    }

    private bool ShouldPlayWelcomeDialogue()
    {
        if (string.IsNullOrWhiteSpace(welcomeDialogueID) ||
            DialogueManager.Instance == null)
        {
            return false;
        }

        if (GameProgressionManager.Instance == null)
            return true;

        return !GameProgressionManager.Instance.HasFlag(
            welcomeProgressionFlag);
    }

    private void GrantWelcomeGift()
    {
        if (welcomeGiftAmount <= 0)
            return;

        if (welcomeGiftItem == null)
            welcomeGiftItem = Resources.Load<ItemData>("Items/Health Potion");

        if (welcomeGiftItem == null)
        {
            Debug.LogWarning(
                "ShopInteractionResponse could not find the welcome gift item.");
            return;
        }

        if (InventorySystem.Instance == null ||
            !InventorySystem.Instance.Add(welcomeGiftItem, welcomeGiftAmount))
        {
            Debug.LogWarning(
                "ShopInteractionResponse could not add the welcome gift to inventory.");
        }
    }

    private string GetNextRepeatDialogueID()
    {
        if (repeatDialogueIDs == null ||
            repeatDialogueIDs.Length == 0)
        {
            return string.Empty;
        }

        if (repeatDialogueBag.Count == 0)
        {
            foreach (string dialogueID in repeatDialogueIDs)
            {
                if (!string.IsNullOrWhiteSpace(dialogueID))
                    repeatDialogueBag.Add(dialogueID);
            }
        }

        if (repeatDialogueBag.Count == 0)
            return string.Empty;

        int index = Random.Range(0, repeatDialogueBag.Count);
        string selectedDialogueID = repeatDialogueBag[index];
        repeatDialogueBag.RemoveAt(index);

        return selectedDialogueID;
    }

    private void OpenShop()
    {
        if (GameplayUIManager.Instance == null ||
            GameplayUIManager.Instance.Shop == null)
        {
            Debug.LogError("ShopInteractionResponse could not find ShopUI.");
            return;
        }

        GameplayUIManager.Instance.Shop.OpenShop();
    }
}
