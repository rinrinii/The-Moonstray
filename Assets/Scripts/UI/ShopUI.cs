using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopUI : MonoBehaviour
{
    private const int HealthPotionCost = 30;
    private const int DocumentCost = 50;

    private VisualElement shopRoot;
    private VisualElement shopWindow;
    private VisualElement shopSlotContainer;
    private VisualElement selectedIcon;
    private Label selectedNameLabel;
    private Label selectedDescriptionLabel;
    private Label selectedPriceLabel;
    private Label walletLabel;
    private Label statusLabel;
    private Button buyButton;
    private Button closeButton;

    private readonly List<ShopOffer> offers = new();
    private readonly HashSet<NoteData> purchasedDocuments = new();
    private readonly PlayerMovementFreezeHandle movementFreeze = new();
    private bool walletSubscribed;

    private ShopOffer selectedOffer;
    private ItemData healthPotion;
    private Sprite documentIcon;

    public static ShopUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshReferences();
        BuildOffers();
        RenderShop();
        CloseShop();
    }

    private void OnEnable()
    {
        SubscribeWallet();
    }

    private void OnDisable()
    {
        if (MoonCoinWallet.Instance != null && walletSubscribed)
            MoonCoinWallet.Instance.OnMoonCoinsChanged -= HandleMoonCoinsChanged;

        walletSubscribed = false;
    }

    public void OpenShop()
    {
        if (shopRoot == null)
            RefreshReferences();

        if (shopRoot == null || shopWindow == null)
        {
            Debug.LogError("ShopUI references missing.");
            return;
        }

        GameplayUIManager.Instance?.SuppressSecondaryPanels(this);

        shopRoot.style.display = DisplayStyle.Flex;
        shopRoot.pickingMode = PickingMode.Position;
        shopWindow.style.display = DisplayStyle.Flex;
        shopWindow.pickingMode = PickingMode.Position;
        movementFreeze.Acquire();
        SubscribeWallet();

        RefreshWallet();
        RenderShop();

        if (selectedOffer == null && offers.Count > 0)
            SelectOffer(offers[0]);
        else
            RefreshDetails();
    }

    public void CloseShop()
    {
        if (shopWindow != null)
        {
            shopWindow.style.display = DisplayStyle.None;
            shopWindow.pickingMode = PickingMode.Ignore;
        }

        if (shopRoot != null)
        {
            shopRoot.style.display = DisplayStyle.None;
            shopRoot.pickingMode = PickingMode.Ignore;
        }

        movementFreeze.Release();
    }

    private void RefreshReferences()
    {
        GameplayUIManager ui = GameplayUIManager.Instance;

        if (ui == null)
            return;

        shopRoot = ui.ShopRoot;

        if (shopRoot == null)
            return;

        shopWindow = shopRoot.Q<VisualElement>("ShopWindow");
        shopSlotContainer = shopRoot.Q<VisualElement>("ShopSlotContainer");
        selectedIcon = shopRoot.Q<VisualElement>("ShopSelectedIcon");
        selectedNameLabel = shopRoot.Q<Label>("ShopSelectedName");
        selectedDescriptionLabel = shopRoot.Q<Label>("ShopSelectedDescription");
        selectedPriceLabel = shopRoot.Q<Label>("ShopSelectedPrice");
        walletLabel = shopRoot.Q<Label>("ShopWalletLabel");
        statusLabel = shopRoot.Q<Label>("ShopStatusLabel");
        buyButton = shopRoot.Q<Button>("ShopBuyButton");
        closeButton = shopRoot.Q<Button>("ShopCloseButton");

        if (buyButton != null)
        {
            buyButton.clicked -= BuySelectedOffer;
            buyButton.clicked += BuySelectedOffer;
        }

        if (closeButton != null)
        {
            closeButton.clicked -= CloseShop;
            closeButton.clicked += CloseShop;
        }
    }

    private void BuildOffers()
    {
        if (offers.Count > 0)
            return;

        healthPotion = Resources.Load<ItemData>("Items/Health Potion");
        ItemData book = Resources.Load<ItemData>("Items/Book");
        documentIcon = book != null ? book.icon : null;

        if (healthPotion != null)
        {
            offers.Add(ShopOffer.CreateHealthPotion(
                healthPotion,
                HealthPotionCost));
        }
        else
        {
            Debug.LogWarning("ShopUI: Health Potion item not found in Resources/Items.");
        }

        NoteData[] notes = Resources.LoadAll<NoteData>("Notes/Shop Notes");
        System.Array.Sort(notes, (a, b) =>
            string.Compare(a.title, b.title, System.StringComparison.OrdinalIgnoreCase));

        foreach (NoteData note in notes)
        {
            if (note == null)
                continue;

            offers.Add(ShopOffer.CreateDocument(
                note,
                DocumentCost,
                documentIcon));

            if (offers.Count >= 5)
                break;
        }
    }

    private void RenderShop()
    {
        if (shopSlotContainer == null)
            return;

        shopSlotContainer.Clear();

        foreach (ShopOffer offer in offers)
            shopSlotContainer.Add(CreateSlot(offer));

        RefreshWallet();
    }

    private VisualElement CreateSlot(ShopOffer offer)
    {
        Button slot = new();
        slot.name = $"ShopSlot-{offer.DisplayName}";
        slot.style.width = 116;
        slot.style.height = 132;
        slot.style.marginRight = 12;
        slot.style.marginBottom = 12;
        slot.style.backgroundColor = new Color(0f, 0.05f, 0.1f, 0.72f);
        slot.style.borderTopWidth = 2;
        slot.style.borderRightWidth = 2;
        slot.style.borderBottomWidth = 2;
        slot.style.borderLeftWidth = 2;
        slot.style.borderTopColor = new Color(0.75f, 0.58f, 0.26f);
        slot.style.borderRightColor = new Color(0.75f, 0.58f, 0.26f);
        slot.style.borderBottomColor = new Color(0.75f, 0.58f, 0.26f);
        slot.style.borderLeftColor = new Color(0.75f, 0.58f, 0.26f);
        slot.style.flexDirection = FlexDirection.Column;
        slot.style.alignItems = Align.Center;
        slot.style.justifyContent = Justify.Center;
        slot.text = string.Empty;

        VisualElement icon = new();
        icon.style.width = 74;
        icon.style.height = 74;

        if (offer.Icon != null)
            icon.style.backgroundImage = new StyleBackground(offer.Icon.texture);

        slot.Add(icon);
        slot.clicked += () => SelectOffer(offer);

        return slot;
    }

    private void SelectOffer(ShopOffer offer)
    {
        selectedOffer = offer;
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (selectedOffer == null)
        {
            ClearDetails();
            return;
        }

        if (selectedNameLabel != null)
            selectedNameLabel.text = selectedOffer.DisplayName;

        if (selectedDescriptionLabel != null)
        {
            selectedDescriptionLabel.text =
                selectedOffer.Kind == ShopOfferKind.Document
                    ? string.Empty
                    : selectedOffer.Description;

            selectedDescriptionLabel.style.display =
                string.IsNullOrWhiteSpace(selectedDescriptionLabel.text)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }

        if (selectedPriceLabel != null)
            selectedPriceLabel.text =
                IsSoldOut(selectedOffer)
                    ? "Already in Journal"
                    : $"{selectedOffer.Price} Moon Coins";

        if (selectedIcon != null)
        {
            bool showIcon = selectedOffer.Icon != null;

            selectedIcon.style.display =
                showIcon ? DisplayStyle.Flex : DisplayStyle.None;

            selectedIcon.style.backgroundImage =
                showIcon
                    ? new StyleBackground(selectedOffer.Icon.texture)
                    : null;
        }

        RefreshBuyButton();
    }

    private void ClearDetails()
    {
        if (selectedNameLabel != null) selectedNameLabel.text = string.Empty;
        if (selectedDescriptionLabel != null) selectedDescriptionLabel.text = string.Empty;
        if (selectedPriceLabel != null) selectedPriceLabel.text = string.Empty;
        if (statusLabel != null) statusLabel.text = string.Empty;
        if (selectedIcon != null) selectedIcon.style.backgroundImage = null;
        if (buyButton != null) buyButton.SetEnabled(false);
    }

    private void RefreshBuyButton()
    {
        if (buyButton == null || selectedOffer == null)
            return;

        bool soldOut = IsSoldOut(selectedOffer);
        bool canAfford =
            MoonCoinWallet.Instance != null &&
            MoonCoinWallet.Instance.MoonCoins >= selectedOffer.Price;

        buyButton.text = soldOut ? "Purchased" : "Buy";
        buyButton.style.display =
            soldOut ? DisplayStyle.None : DisplayStyle.Flex;
        buyButton.SetEnabled(!soldOut && canAfford);

        if (statusLabel == null)
            return;

        statusLabel.text = soldOut
            ? string.Empty
            : canAfford
                ? string.Empty
                : "Not enough Moon Coins.";
    }

    private void BuySelectedOffer()
    {
        if (selectedOffer == null || IsSoldOut(selectedOffer))
            return;

        if (MoonCoinWallet.Instance == null ||
            MoonCoinWallet.Instance.MoonCoins < selectedOffer.Price)
        {
            if (statusLabel != null)
                statusLabel.text = "Not enough Moon Coins.";

            RefreshBuyButton();
            return;
        }

        if (selectedOffer.Kind == ShopOfferKind.HealthPotion)
        {
            if (InventorySystem.Instance == null ||
                !InventorySystem.Instance.Add(selectedOffer.Item))
            {
                if (statusLabel != null)
                    statusLabel.text = "Inventory is full.";

                return;
            }
        }
        else if (selectedOffer.Note != null)
        {
            if (JournalController.Instance == null)
            {
                if (statusLabel != null)
                    statusLabel.text = "Journal is unavailable.";

                return;
            }

            purchasedDocuments.Add(selectedOffer.Note);
            JournalController.Instance?.AddNote(
                selectedOffer.Note.title,
                selectedOffer.Note.content);
        }

        MoonCoinWallet.Instance.Spend(selectedOffer.Price);

        if (statusLabel != null)
            statusLabel.text = "Purchased.";

        RenderShop();
        RefreshDetails();
    }

    private bool IsSoldOut(ShopOffer offer)
    {
        return offer.Kind == ShopOfferKind.Document &&
            offer.Note != null &&
            purchasedDocuments.Contains(offer.Note);
    }

    private void HandleMoonCoinsChanged(int amount)
    {
        RefreshWallet();
        RefreshBuyButton();
    }

    private void RefreshWallet()
    {
        if (walletLabel == null)
            return;

        int coins = MoonCoinWallet.Instance != null
            ? MoonCoinWallet.Instance.MoonCoins
            : 0;

        walletLabel.text = $"{coins} Moon Coins";
    }

    private void SubscribeWallet()
    {
        if (walletSubscribed || MoonCoinWallet.Instance == null)
            return;

        MoonCoinWallet.Instance.OnMoonCoinsChanged += HandleMoonCoinsChanged;
        walletSubscribed = true;
    }

    private enum ShopOfferKind
    {
        HealthPotion,
        Document
    }

    private class ShopOffer
    {
        public ShopOfferKind Kind;
        public string DisplayName;
        public string Description;
        public int Price;
        public Sprite Icon;
        public ItemData Item;
        public NoteData Note;

        public static ShopOffer CreateHealthPotion(
            ItemData item,
            int price)
        {
            return new ShopOffer
            {
                Kind = ShopOfferKind.HealthPotion,
                DisplayName = item.itemName,
                Description = item.description,
                Price = price,
                Icon = item.icon,
                Item = item
            };
        }

        public static ShopOffer CreateDocument(
            NoteData note,
            int price,
            Sprite icon)
        {
            return new ShopOffer
            {
                Kind = ShopOfferKind.Document,
                DisplayName = note.title,
                Description = note.content,
                Price = price,
                Icon = icon,
                Note = note
            };
        }
    }
}
