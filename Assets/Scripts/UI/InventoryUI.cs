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

    [SerializeField]
    private bool unlocked = false;
    public bool IsUnlocked => unlocked;

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
            ClearMockElements();
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
        closeButton = root.Q<Button>("CloseButton");

        if (itemGrid != null) itemGrid.pickingMode = PickingMode.Position;
        if (fragmentGrid != null) fragmentGrid.pickingMode = PickingMode.Position;

        // TEMP DEBUG: confirm every reference was actually found
        if (itemGrid == null) Debug.LogWarning("InventoryUI: itemGrid (ItemSlotLayer) NOT FOUND");
        if (fragmentGrid == null) Debug.LogWarning("InventoryUI: fragmentGrid (FragmentSlotLayer) NOT FOUND");
        if (useButton == null) Debug.LogWarning("InventoryUI: useButton NOT FOUND");
        if (nameLabel == null) Debug.LogWarning("InventoryUI: nameLabel NOT FOUND");
    }

    void HookButtons()
    {
        if (closeButton != null) closeButton.clicked += CloseInventory;
        if (useButton != null) useButton.clicked += UseItem;
    }

    void ClearMockElements()
    {
        itemGrid?.Clear();
        fragmentGrid?.Clear();
        ClearInfoPanel();
    }

    void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
        {
            if (isInventoryOpen) CloseInventory();
            return;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
        {
            if (isInventoryOpen)
                CloseInventory();

            return;
        }

        if (unlocked && Input.GetKeyDown(KeyCode.I))
        {
            bool actuallyOpen =
                inventoryRoot != null &&
                inventoryRoot.style.display == DisplayStyle.Flex;

            if (actuallyOpen) CloseInventory();
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
        if (!unlocked)
            return;

        if (inventoryRoot == null || inventoryWindow == null) return;

        // FIX: pass `this` so SuppressSecondaryPanels doesn't also close
        // the inventory it was just asked to open.
        GameplayUIManager.Instance.SuppressSecondaryPanels(this);

        isInventoryOpen = true;

        inventoryRoot.style.display = DisplayStyle.Flex;
        inventoryWindow.style.display = DisplayStyle.Flex;

        inventoryRoot.pickingMode = PickingMode.Position;
        inventoryWindow.pickingMode = PickingMode.Position;

        RefreshUI();
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;

        if (inventoryWindow != null)
        {
            inventoryWindow.style.display = DisplayStyle.None;
            inventoryWindow.pickingMode = PickingMode.Ignore;
        }

        if (inventoryRoot != null)
        {
            inventoryRoot.style.display = DisplayStyle.None;
            inventoryRoot.pickingMode = PickingMode.Ignore;
        }
    }

    public bool IsInventoryActive()
    {
        return inventoryRoot != null &&
               inventoryRoot.style.display == DisplayStyle.Flex;
    }

    public void RefreshUI()
    {
        if (itemGrid == null || fragmentGrid == null || InventorySystem.Instance == null)
        {
            Debug.LogWarning($"InventoryUI: RefreshUI bailed early. itemGrid null={itemGrid == null}, fragmentGrid null={fragmentGrid == null}, InventorySystem.Instance null={InventorySystem.Instance == null}");
            return;
        }

        itemGrid.Clear();
        fragmentGrid.Clear();

        Debug.Log($"InventoryUI: RefreshUI running, slots.Count={InventorySystem.Instance.slots.Count}");

        bool selectedStillExists = false;

        foreach (var slot in InventorySystem.Instance.slots)
        {
            if (slot.item == null) continue;

            if (slot.item == selectedItem)
                selectedStillExists = true;

            VisualElement ui;
            try
            {
                ui = CreateSlot(slot.item, slot.amount);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InventoryUI: CreateSlot threw for item '{slot.item.itemName}': {e}");
                continue;
            }

            Debug.Log($"InventoryUI: adding '{slot.item.itemName}' (type={slot.item.type}) to {(slot.item.type == ItemType.Fragment ? "fragmentGrid" : "itemGrid")}");

            if (slot.item.type == ItemType.Fragment)
                fragmentGrid.Add(ui);
            else
                itemGrid.Add(ui);
        }

        if (!selectedStillExists)
            ClearInfoPanel();
    }

    void ClearInfoPanel()
    {
        selectedItem = null;
        if (nameLabel != null) nameLabel.text = "";
        if (descLabel != null) descLabel.text = "";
        if (icon != null) icon.style.backgroundImage = null;
        if (useButton != null) useButton.style.display = DisplayStyle.None;
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

    public void Unlock()
    {
        if (unlocked)
            return;

        unlocked = true;

        Debug.Log($"{nameof(InventoryUI)} unlocked.");
    }
}