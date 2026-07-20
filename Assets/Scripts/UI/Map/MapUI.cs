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

    [SerializeField] private Transform player;
    private VisualElement mapPlayerArrow;
    private MapData currentMapData;

    [SerializeField]
    private bool hasMap = false;
    public bool HasMap => hasMap;

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

    public MapData CurrentMapData => currentMapData;
    public VisualElement MapImageContainer => mapImageContainer;
    public Sprite CurrentSceneMap => currentSceneMap;
    public float CurrentZoom => currentZoom;
    public Vector2 MapOffset => mapOffset;
    public bool ShowingWorldMap => showingWorldMap;

    private void Start()
    {
        RefreshReferences();
        BindControlCallbacks();

        SetupMapImage();
        LoadSceneMap();

        if (mapRoot != null)
            mapRoot.style.display = DisplayStyle.None;

        if (worldMapBtn != null)
        {
            worldMapBtn.style.display =
                hasMap ? DisplayStyle.Flex : DisplayStyle.None;
        }

        mapOpen = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (worldMapBtn != null)
            worldMapBtn.clicked -= OnWorldMapTogglePressed;

        if (closeButton != null)
            closeButton.clicked -= CloseMap;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
        BindControlCallbacks();
        SetupMapImage();
        LoadSceneMap();
        CloseMap();
    }

    private void RefreshReferences()
    {
        var ui = GameplayUIManager.Instance;

        if (ui == null)
            return;

        mapRoot = ui.MapRoot;
        VisualElement root = ui.RootVisualElement;

        if (root == null)
            return;

        mapImageContainer = root.Q<VisualElement>("MapImageContainer");
        mapImage = root.Q<Image>("MapImage");
        worldMapBtn = root.Q<Button>("WorldMap-Button");
        closeButton = mapRoot?.Q<Button>("CloseButton");
        regionTitleLabel = root.Q<Label>("regionTitle-Label");
        mapPlayerArrow = root.Q<VisualElement>("MapPlayerArrow");
    }

    private void BindControlCallbacks()
    {
        if (worldMapBtn != null)
        {
            worldMapBtn.clicked -= OnWorldMapTogglePressed;
            worldMapBtn.clicked += OnWorldMapTogglePressed;
        }

        if (closeButton != null)
        {
            closeButton.clicked -= CloseMap;
            closeButton.clicked += CloseMap;
        }
    }

    private void Update()
    {
        // =========================================
        // DEBUG
        // =========================================

        // Instantly unlock the map. This shortcut is
        // intentionally available in development builds
        // for testing later tutorial sections.
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Unlock();
        }

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            if (mapOpen)
                CloseMap();

            return;
        }

        if (GameplayUIManager.Instance != null &&
            GameplayUIManager.Instance.IsPuzzleViewActive)
        {
            if (mapOpen)
                CloseMap();

            return;
        }

        if (hasMap && Input.GetKeyDown(KeyCode.M))
        {
            if (mapOpen)
                CloseMap();
            else
                OpenMap();
        }

        if (!mapOpen)
            return;

        UpdatePlayerMarker();
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

        if (mapImage == null)
            return;

        mapImage.scaleMode = ScaleMode.ScaleToFit;
        mapImage.style.width = Length.Percent(100);
        mapImage.style.height = Length.Percent(100);

        mapImage.style.transformOrigin = new TransformOrigin(
            Length.Percent(50),
            Length.Percent(50),
            0);
    }

    public void Unlock()
    {
        if (hasMap)
            return;

        hasMap = true;

        if (worldMapBtn != null)
            worldMapBtn.style.display = DisplayStyle.Flex;

        Debug.Log($"{nameof(MapUI)} unlocked.");
    }

    public void OpenMap()
    {
        if (!hasMap ||
            (GameplayUIManager.Instance != null &&
             GameplayUIManager.Instance.IsPuzzleViewActive))
            return;

        RefreshReferences();
        LoadSceneMap();

        if (mapRoot == null)
            return;

        GameplayUIManager.Instance.SuppressSecondaryPanels(this);

        mapOpen = true;
        mapRoot.style.display = DisplayStyle.Flex;
        mapRoot.pickingMode = PickingMode.Position;

        ResetZoomAndPan();
    }

    public void CloseMap()
    {
        if (mapRoot == null)
            return;

        mapOpen = false;
        dragging = false;
        mapRoot.style.display = DisplayStyle.None;
    }

    private void LoadSceneMap()
    {
        string scene = SceneManager.GetActiveScene().name;
        MapData map = FindSceneMap(scene);
        currentMapData = map;

        if (map != null)
        {
            currentSceneMap = map.mapSprite;
            currentRegionTitle = string.IsNullOrWhiteSpace(map.sceneName)
                ? scene
                : map.sceneName;

            showingWorldMap = false;

            if (currentSceneMap == null)
                Debug.LogWarning(
                    $"MapUI: Map data for scene '{scene}' has no sprite assigned.");

            SetMap(currentSceneMap);
            SetRegionTitle(currentRegionTitle);
            SetWorldMapButtonText("World Map");

            return;
        }

        Debug.LogWarning($"MapUI: No map assigned for scene '{scene}'.");
        currentSceneMap = null;
        currentRegionTitle = scene;
        showingWorldMap = false;
        ClearMap();
        SetRegionTitle(scene);
        SetWorldMapButtonText("World Map");
    }

    private MapData FindSceneMap(string scene)
    {
        if (sceneMaps != null)
        {
            foreach (MapData map in sceneMaps)
            {
                if (map != null && map.sceneName == scene)
                    return map;
            }
        }

        MapData[] resourceMaps = Resources.LoadAll<MapData>("Map");

        foreach (MapData map in resourceMaps)
        {
            if (map != null && map.sceneName == scene)
                return map;
        }

        return null;
    }

    private void SetMap(Sprite sprite)
    {
        if (mapImage == null)
            return;

        if (sprite == null)
        {
            ClearMap();
            return;
        }

        mapImage.image = sprite.texture;
        mapImage.scaleMode = ScaleMode.ScaleToFit;

        ResetZoomAndPan();
    }

    private void ClearMap()
    {
        if (mapImage != null)
            mapImage.image = null;

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

    private void UpdatePlayerMarker()
    {
        if (player == null ||
            currentMapData == null ||
            currentSceneMap == null ||
            mapImageContainer == null ||
            mapPlayerArrow == null)
        {
            return;
        }

        float containerWidth =
            mapImageContainer.resolvedStyle.width;

        float containerHeight =
            mapImageContainer.resolvedStyle.height;

        if (containerWidth <= 0f ||
            containerHeight <= 0f)
        {
            return;
        }

        float spriteWidth =
            currentSceneMap.rect.width;

        float spriteHeight =
            currentSceneMap.rect.height;

        float spriteAspect =
            spriteWidth / spriteHeight;

        float containerAspect =
            containerWidth / containerHeight;

        float displayedWidth;
        float displayedHeight;
        float imageOffsetX;
        float imageOffsetY;

        if (spriteAspect > containerAspect)
        {
            displayedWidth = containerWidth;
            displayedHeight = displayedWidth / spriteAspect;

            imageOffsetX = 0f;
            imageOffsetY =
                (containerHeight - displayedHeight) * 0.5f;
        }
        else
        {
            displayedHeight = containerHeight;
            displayedWidth = displayedHeight * spriteAspect;

            imageOffsetX =
                (containerWidth - displayedWidth) * 0.5f;

            imageOffsetY = 0f;
        }

        float normalizedX =
            Mathf.InverseLerp(
                currentMapData.worldMin.x,
                currentMapData.worldMax.x,
                player.position.x
            );

        float normalizedY =
            Mathf.InverseLerp(
                currentMapData.worldMin.y,
                currentMapData.worldMax.y,
                player.position.z
            );

        float markerX =
            imageOffsetX +
            normalizedX * displayedWidth;

        float markerY =
            imageOffsetY +
            (1f - normalizedY) * displayedHeight;

        float centerX = containerWidth * 0.5f;
        float centerY = containerHeight * 0.5f;

        markerX =
            centerX +
            (markerX - centerX) * currentZoom +
            mapOffset.x;

        markerY =
            centerY +
            (markerY - centerY) * currentZoom +
            mapOffset.y;

        float markerWidth =
            mapPlayerArrow.resolvedStyle.width;

        float markerHeight =
            mapPlayerArrow.resolvedStyle.height;

        mapPlayerArrow.style.left =
            markerX - markerWidth * 0.5f;

        mapPlayerArrow.style.top =
            markerY - markerHeight * 0.5f;
    }
}
