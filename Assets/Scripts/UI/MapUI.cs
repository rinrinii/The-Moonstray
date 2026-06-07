using UnityEngine;
using UnityEngine.UIElements;

public class MapUI : MonoBehaviour
{
    [Header("World Bounds (XZ)")]
    public Vector2 worldMinXZ;
    public Vector2 worldMaxXZ;

    [Header("Player")]
    public Transform player;

    private VisualElement mapRoot;
    private VisualElement mapImage;
    private VisualElement playerIcon;
    private Button worldMapBtn;

    private bool mapOpen;

    private void Start()
    {
        var ui = GameplayUIManager.Instance;
        mapRoot = ui.MapRoot;
        
        VisualElement root = ui.RootVisualElement;
        mapImage = root.Q<VisualElement>("MapImage");
        playerIcon = root.Q<VisualElement>("PlayerIcon");
        worldMapBtn = root.Q<Button>("WorldMap-Button");

        if (worldMapBtn != null) worldMapBtn.clicked += OnWorldMapTogglePressed;

        if (mapRoot != null) mapRoot.style.display = DisplayStyle.None;
        mapOpen = false;
    }

    private void OnDestroy()
    {
        if (worldMapBtn != null) worldMapBtn.clicked -= OnWorldMapTogglePressed;
    }

    private void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused()) 
        {
            if (mapOpen) CloseMap();
            return;
        }

        HandleInput();

        if (!mapOpen || player == null || mapImage == null || playerIcon == null) return;

        UpdatePlayerIcon();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapOpen) CloseMap();
            else OpenMap();
        }
    }

    public void OpenMap()
    {
        if (mapRoot == null) return;
        GameplayUIManager.Instance.SuppressSecondaryPanels(); // Force conflicting overlays to hide
        mapOpen = true;
        mapRoot.style.display = DisplayStyle.Flex;
    }

    public void CloseMap()
    {
        if (mapRoot == null) return;
        mapOpen = false;
        mapRoot.style.display = DisplayStyle.None;
    }

    private void UpdatePlayerIcon()
    {
        float w = mapImage.resolvedStyle.width;
        float h = mapImage.resolvedStyle.height;

        if (w <= 1 || h <= 1) return;

        Vector3 p = player.position;
        float x = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, p.x);
        float y = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, p.z);

        float px = x * w - (playerIcon.resolvedStyle.width / 2f);
        float py = (1f - y) * h - (playerIcon.resolvedStyle.height / 2f);

        playerIcon.style.left = px;
        playerIcon.style.top = py;
    }

    private void OnWorldMapTogglePressed() => Debug.Log("Swapping active map configurations...");
    public bool IsMapActive() => mapOpen;
}