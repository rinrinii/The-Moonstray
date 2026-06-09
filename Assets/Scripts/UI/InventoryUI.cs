using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset itemSlotTemplate;

    private VisualElement inventoryRoot;
    private VisualElement inventoryWindow;
    private bool isInventoryOpen = false;

    private VisualElement itemGrid;
    private VisualElement fragmentGrid;
    private VisualElement infoPanel;
    private Label nameLabel;
    private Label descLabel;
    private VisualElement icon;
    private Button useButton;
    private Button closeButton;

    private ItemData selectedItem;

    public static InventoryUI Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        var ui = GameplayUIManager.Instance;
        inventoryRoot = ui.InventoryRoot;

        if (inventoryRoot != null)
        {
            BindUI(inventoryRoot);
            HookButtons();
            CloseInventory();
        }

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged += RefreshUI;
    }

    void BindUI(VisualElement root)
    {
        root.pickingMode = PickingMode.Ignore;

        inventoryWindow = root.Q<VisualElement>("InventoryWindow");
        if (inventoryWindow != null)
            inventoryWindow.pickingMode = PickingMode.Position;

        VisualElement searchRoot = inventoryWindow ?? root;

        itemGrid = searchRoot.Q<VisualElement>("ItemSlotLayer");
        fragmentGrid = searchRoot.Q<VisualElement>("FragmentSlotLayer");
        infoPanel = searchRoot.Q<VisualElement>("InfoPanel");
        nameLabel = searchRoot.Q<Label>("ItemNameLabel");
        descLabel = searchRoot.Q<Label>("ItemDescriptionLabel");
        icon = searchRoot.Q<VisualElement>("ItemIcon");
        useButton = searchRoot.Q<Button>("UseButton");
        closeButton = searchRoot.Q<Button>("CloseButton");

        if (itemGrid != null) itemGrid.pickingMode = PickingMode.Position;
        if (fragmentGrid != null) fragmentGrid.pickingMode = PickingMode.Position;
    }

    void HookButtons()
    {
        if (closeButton != null) closeButton.clicked += CloseInventory;
        if (useButton != null) useButton.clicked += UseItem;
    }

    void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
        {
            if (isInventoryOpen) CloseInventory();
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isInventoryOpen) CloseInventory();
            else Open();
        }
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
    }

    public void Open()
    {
        if (inventoryRoot == null || inventoryWindow == null) return;

        if (GameplayUIManager.Instance != null)
            GameplayUIManager.Instance.SuppressSecondaryPanels();

        isInventoryOpen = true;
        inventoryWindow.pickingMode = PickingMode.Position;
        inventoryRoot.style.display = DisplayStyle.Flex;
        inventoryWindow.style.display = DisplayStyle.Flex;
        RefreshUI();
    }

    public void CloseInventory()
    {
        if (inventoryRoot == null) return;
        isInventoryOpen = false;
        if (inventoryWindow != null)
            inventoryWindow.pickingMode = PickingMode.Ignore;
        inventoryRoot.style.display = DisplayStyle.None;
    }

    public bool IsInventoryActive() => isInventoryOpen;

    public void RefreshUI()
    {
        if (itemGrid == null || fragmentGrid == null || InventorySystem.Instance == null) return;

        itemGrid.Clear();
        fragmentGrid.Clear();

        foreach (var slot in InventorySystem.Instance.slots)
        {
            if (slot.item == null) continue;

            var ui = CreateSlot(slot.item, slot.amount);

            if (slot.item.type == ItemType.Fragment)
                fragmentGrid.Add(ui);
            else
                itemGrid.Add(ui);
        }
    }

    VisualElement CreateSlot(ItemData item, int amount)
    {
        var slotTemplateInstance = itemSlotTemplate.Instantiate();
        VisualElement slotRoot = slotTemplateInstance.Q<VisualElement>("ItemSlotTemplate");

        if (slotRoot == null)
            slotRoot = slotTemplateInstance.ElementAt(0);

        slotRoot.pickingMode = PickingMode.Position;

        var iconEl = slotRoot.Q<VisualElement>("ItemIcon");
        var qtyEl = slotRoot.Q<Label>("ItemQuantity");

        if (iconEl != null && item.icon != null)
            iconEl.style.backgroundImage = new StyleBackground(item.icon.texture);

        if (qtyEl != null)
            qtyEl.text = amount > 1 ? amount.ToString() : "";

        slotRoot.RegisterCallback<ClickEvent>(_ => SelectItem(item));

        return slotRoot;
    }

    void SelectItem(ItemData item)
    {
        selectedItem = item;

        if (nameLabel != null) nameLabel.text = item.itemName;
        if (descLabel != null) descLabel.text = item.description;
        if (icon != null && item.icon != null)
            icon.style.backgroundImage = new StyleBackground(item.icon.texture);

        if (useButton != null)
            useButton.style.display = item.canUse ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void UseItem()
    {
        if (selectedItem == null || InventorySystem.Instance == null) return;

        InventorySystem.Instance.Remove(selectedItem);
        RefreshUI();
    }
}