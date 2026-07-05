using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MapUI : MonoBehaviour
{
    [Header("Maps")]
    [SerializeField] private Sprite worldMapSprite;
    [SerializeField] private string worldMapTitle = "World Map";
    [SerializeField] private MapData[] sceneMaps;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 2.5f;
    [SerializeField] private float zoomSpeed = 0.2f;

    private VisualElement mapRoot;
    private VisualElement mapImageContainer;
    private Image mapImage;
    private Button worldMapBtn;
    private Button closeButton;
    private Label regionTitleLabel;

    private bool mapOpen;
    private bool showingWorldMap;

    private Sprite currentSceneMap;
    private string currentRegionTitle;

    private float currentZoom = 1f;
    private Vector2 mapOffset = Vector2.zero;
    private Vector2 dragStart;
    private bool dragging;

    private void Start()
    {
        var ui = GameplayUIManager.Instance;

        mapRoot = ui.MapRoot;
        VisualElement root = ui.RootVisualElement;

        mapImageContainer = root.Q<VisualElement>("MapImageContainer");
        mapImage = root.Q<Image>("MapImage");
        worldMapBtn = root.Q<Button>("WorldMap-Button");
        closeButton = mapRoot?.Q<Button>("CloseButton");
        regionTitleLabel = root.Q<Label>("regionTitle-Label");

        if (worldMapBtn != null)
            worldMapBtn.clicked += OnWorldMapTogglePressed;

        if (closeButton != null)
            closeButton.clicked += CloseMap;

        SetupMapImage();
        LoadSceneMap();

        if (mapRoot != null)
            mapRoot.style.display = DisplayStyle.None;

        mapOpen = false;
    }

    private void OnDestroy()
    {
        if (worldMapBtn != null)
            worldMapBtn.clicked -= OnWorldMapTogglePressed;

        if (closeButton != null)
            closeButton.clicked -= CloseMap;
    }

    private void Update()
    {
        if (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused())
        {
            if (mapOpen) CloseMap();
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapOpen) CloseMap();
            else OpenMap();
        }

        if (!mapOpen)
            return;

        HandleZoom();
        HandleDrag();
    }

    private void SetupMapImage()
    {
        if (mapImageContainer != null)
        {
            mapImageContainer.style.overflow = Overflow.Hidden;
            mapImageContainer.pickingMode = PickingMode.Position;
        }

        if (mapImage == null) return;

        mapImage.scaleMode = ScaleMode.ScaleToFit;
        mapImage.style.width = Length.Percent(100);
        mapImage.style.height = Length.Percent(100);

        mapImage.style.transformOrigin = new TransformOrigin(
            Length.Percent(50),
            Length.Percent(50),
            0
        );
    }

    public void OpenMap()
    {
        if (mapRoot == null) return;

        GameplayUIManager.Instance.SuppressSecondaryPanels();

        mapOpen = true;
        mapRoot.style.display = DisplayStyle.Flex;
        mapRoot.pickingMode = PickingMode.Position;

        ResetZoomAndPan();
    }

    public void CloseMap()
    {
        if (mapRoot == null) return;

        mapOpen = false;
        dragging = false;
        mapRoot.style.display = DisplayStyle.None;
    }

    private void LoadSceneMap()
    {
        string scene = SceneManager.GetActiveScene().name;

        foreach (MapData map in sceneMaps)
        {
            if (map.sceneName == scene)
            {
                currentSceneMap = map.mapSprite;
                currentRegionTitle = map.regionTitle;

                showingWorldMap = false;
                SetMap(currentSceneMap);
                SetRegionTitle(currentRegionTitle);
                SetWorldMapButtonText("World Map");

                return;
            }
        }

        Debug.LogWarning($"MapUI: No map assigned for scene '{scene}'.");
        SetRegionTitle(scene);
    }

    private void SetMap(Sprite sprite)
    {
        if (sprite == null || mapImage == null) return;

        mapImage.image = sprite.texture;
        mapImage.scaleMode = ScaleMode.ScaleToFit;

        ResetZoomAndPan();
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        currentZoom += scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (Mathf.Approximately(currentZoom, minZoom))
        {
            currentZoom = minZoom;
            mapOffset = Vector2.zero;
            dragging = false;
        }

        ApplyZoomAndPan();
    }

    private void HandleDrag()
    {
        if (currentZoom <= minZoom)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            dragStart = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (!dragging)
            return;

        Vector2 currentMouse = Input.mousePosition;
        Vector2 delta = currentMouse - dragStart;
        dragStart = currentMouse;

        mapOffset += new Vector2(delta.x, -delta.y);
        ApplyZoomAndPan();
    }

    private void ApplyZoomAndPan()
    {
        if (mapImage == null || mapImageContainer == null)
            return;

        float containerW = mapImageContainer.resolvedStyle.width;
        float containerH = mapImageContainer.resolvedStyle.height;

        float maxOffsetX = (containerW * (currentZoom - 1f)) / 2f;
        float maxOffsetY = (containerH * (currentZoom - 1f)) / 2f;

        mapOffset.x = Mathf.Clamp(mapOffset.x, -maxOffsetX, maxOffsetX);
        mapOffset.y = Mathf.Clamp(mapOffset.y, -maxOffsetY, maxOffsetY);

        mapImage.style.scale = new Scale(new Vector2(currentZoom, currentZoom));
        mapImage.style.translate = new Translate(mapOffset.x, mapOffset.y, 0);
    }

    private void ResetZoomAndPan()
    {
        currentZoom = 1f;
        mapOffset = Vector2.zero;
        dragging = false;

        ApplyZoomAndPan();
    }

    private void SetRegionTitle(string title)
    {
        if (regionTitleLabel != null)
            regionTitleLabel.text = title;
    }

    private void SetWorldMapButtonText(string text)
    {
        if (worldMapBtn != null)
            worldMapBtn.text = text;
    }

    private void OnWorldMapTogglePressed()
    {
        showingWorldMap = !showingWorldMap;

        if (showingWorldMap)
        {
            SetMap(worldMapSprite);
            SetRegionTitle(worldMapTitle);
            SetWorldMapButtonText("Region Map");
        }
        else
        {
            SetMap(currentSceneMap);
            SetRegionTitle(currentRegionTitle);
            SetWorldMapButtonText("World Map");
        }
    }

    public bool IsMapActive()
    {
        return mapOpen;
    }
}