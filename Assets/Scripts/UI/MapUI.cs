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

    private bool mapOpen;

    void Start()
    {
        UIDocument doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;

        mapRoot = root.Q<VisualElement>("MapRoot");
        mapImage = root.Q<VisualElement>("MapImage");
        playerIcon = root.Q<VisualElement>("PlayerIcon");

        // START CLOSED
        mapRoot.style.display = DisplayStyle.None;
        mapOpen = false;
    }

    void Update()
    {
        HandleInput();

        if (!mapOpen) return;
        if (player == null) return;

        UpdatePlayerIcon();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapOpen = !mapOpen;

            mapRoot.style.display = mapOpen
                ? DisplayStyle.Flex
                : DisplayStyle.None;

        }
    }

    void UpdatePlayerIcon()
    {
        float w = mapImage.resolvedStyle.width;
        float h = mapImage.resolvedStyle.height;

        if (w <= 1 || h <= 1) return;

        Vector3 p = player.position;

        float x = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, p.x);
        float y = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, p.z);

        float px = x * w - 12;
        float py = (1f - y) * h - 12;

        playerIcon.style.left = px;
        playerIcon.style.top = py;
    }
}