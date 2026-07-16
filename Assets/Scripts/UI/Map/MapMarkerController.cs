using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class MapMarkerController : MonoBehaviour
{
    public static MapMarkerController Instance { get; private set; }

    [Header("Marker Icons")]
    [SerializeField] private Texture2D questIcon;
    [SerializeField] private Texture2D noteIcon;
    [SerializeField] private Texture2D npcIcon;
    [SerializeField] private Texture2D poiIcon;
    [SerializeField] private Texture2D resourceIcon;
    [SerializeField] private Texture2D blockedIcon;
    [SerializeField] private Texture2D shopIcon;
    [SerializeField] private Texture2D questBoardIcon;

    [Header("Marker Size")]
    [SerializeField] private float minimapMarkerSizePercent = 10f;
    [SerializeField] private float fullMapMarkerSizePercent = 4f;

    [SerializeField] private MinimapUI minimapUI;
    [SerializeField] private MapUI mapUI;

    private VisualElement minimapMarkerContainer;
    private VisualElement fullMapMarkerContainer;

    private readonly Dictionary<MapMarkerTarget, VisualElement> minimapMarkers = new();
    private readonly Dictionary<MapMarkerTarget, VisualElement> fullMapMarkers = new();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return null;

        RefreshReferences();
        RefreshMarkers();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshAfterSceneLoad());
    }

    private IEnumerator RefreshAfterSceneLoad()
    {
        yield return null;

        RefreshReferences();
        RefreshMarkers();
    }

    private void RefreshReferences()
    {
        VisualElement root =
            GameplayUIManager.Instance?.RootVisualElement;

        if (root == null)
            return;

        minimapMarkerContainer =
            root.Q<VisualElement>("MinimapMarkerContainer");

        fullMapMarkerContainer =
            root.Q<VisualElement>("MapMarkerContainer");

        Debug.Log($"Minimap container found: {minimapMarkerContainer != null}");
        Debug.Log($"Full map container found: {fullMapMarkerContainer != null}");
    }

    public void RefreshMarkers()
    {
        ClearMarkers();

        MapMarkerTarget[] targets =
            FindObjectsByType<MapMarkerTarget>(
                FindObjectsSortMode.None
            );

        foreach (MapMarkerTarget target in targets)
        {
            if (!target.IsActive)
                continue;

            if (target.ShowOnMinimap &&
                minimapMarkerContainer != null)
            {
                VisualElement marker =
                    CreateMarker(
                        target.MarkerType,
                        minimapMarkerSizePercent
                    );

                minimapMarkerContainer.Add(marker);
                minimapMarkers.Add(target, marker);
            }

            if (target.ShowOnFullMap &&
                fullMapMarkerContainer != null)
            {
                VisualElement marker =
                    CreateMarker(
                        target.MarkerType,
                        fullMapMarkerSizePercent
                    );

                fullMapMarkerContainer.Add(marker);
                fullMapMarkers.Add(target, marker);
            }
        }
    }

    private VisualElement CreateMarker(
        MapMarkerType markerType,
        float sizePercent)
    {
        VisualElement marker = new VisualElement();

        marker.style.position = Position.Absolute;
        marker.style.width = Length.Percent(sizePercent);
        marker.style.height = Length.Percent(sizePercent);
        marker.style.backgroundImage =
            new StyleBackground(GetIcon(markerType));

        marker.style.unityBackgroundScaleMode =
            ScaleMode.ScaleToFit;

        marker.pickingMode = PickingMode.Ignore;

        return marker;
    }

    private Texture2D GetIcon(MapMarkerType markerType)
    {
        switch (markerType)
        {
            case MapMarkerType.Quest:
                return questIcon;

            case MapMarkerType.Note:
                return noteIcon;

            case MapMarkerType.NPC:
                return npcIcon;

            case MapMarkerType.POI:
                return poiIcon;

            case MapMarkerType.Resource:
                return resourceIcon;

            case MapMarkerType.Blocked:
                return blockedIcon;

            case MapMarkerType.Shop:
                return shopIcon;

            case MapMarkerType.QuestBoard:
                return questBoardIcon;

            default:
                return poiIcon;
        }
    }

    private void ClearMarkers()
    {
        minimapMarkerContainer?.Clear();
        fullMapMarkerContainer?.Clear();

        minimapMarkers.Clear();
        fullMapMarkers.Clear();
    }

    private void LateUpdate()
    {
        UpdateMinimapMarkers();
        UpdateFullMapMarkers();
    }

    private void UpdateMinimapMarkers()
    {
        if (minimapUI == null ||
            minimapUI.CurrentMap == null ||
            minimapUI.MinimapContainer == null ||
            minimapUI.MinimapImage == null)
        {
            return;
        }

        MapData mapData = minimapUI.CurrentMap;
        VisualElement container = minimapUI.MinimapContainer;
        Image mapImage = minimapUI.MinimapImage;

        float mapWidth = mapImage.resolvedStyle.width;
        float mapHeight = mapImage.resolvedStyle.height;

        float containerWidth = container.resolvedStyle.width;
        float containerHeight = container.resolvedStyle.height;

        if (mapWidth <= 0f ||
            mapHeight <= 0f ||
            containerWidth <= 0f ||
            containerHeight <= 0f)
        {
            return;
        }

        Vector3 translation =
            mapImage.resolvedStyle.translate;

        foreach (var pair in minimapMarkers)
        {
            MapMarkerTarget target = pair.Key;
            VisualElement marker = pair.Value;

            if (target == null || marker == null)
                continue;

            float normalizedX = Mathf.InverseLerp(
                mapData.worldMin.x,
                mapData.worldMax.x,
                target.WorldPosition.x
            );

            float normalizedY = Mathf.InverseLerp(
                mapData.worldMin.y,
                mapData.worldMax.y,
                target.WorldPosition.z
            );

            float markerX =
                normalizedX * mapWidth +
                translation.x;

            float markerY =
                (1f - normalizedY) * mapHeight +
                translation.y;

            float markerWidth =
                marker.resolvedStyle.width;

            float markerHeight =
                marker.resolvedStyle.height;

            float halfWidth = markerWidth * 0.5f;
            float halfHeight = markerHeight * 0.5f;

            markerX = Mathf.Clamp(
                markerX,
                halfWidth,
                containerWidth - halfWidth
            );

            markerY = Mathf.Clamp(
                markerY,
                halfHeight,
                containerHeight - halfHeight
            );

            marker.style.left =
                markerX - halfWidth;

            marker.style.top =
                markerY - halfHeight;
        }
    }

    private void UpdateFullMapMarkers()
    {
        if (mapUI == null ||
            mapUI.CurrentMapData == null ||
            mapUI.CurrentSceneMap == null ||
            mapUI.MapImageContainer == null)
        {
            return;
        }

        bool markersVisible = !mapUI.ShowingWorldMap;

        if (fullMapMarkerContainer != null)
        {
            fullMapMarkerContainer.style.display =
                markersVisible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        if (!markersVisible)
            return;

        MapData mapData = mapUI.CurrentMapData;
        Sprite mapSprite = mapUI.CurrentSceneMap;
        VisualElement container = mapUI.MapImageContainer;

        float containerWidth =
            container.resolvedStyle.width;

        float containerHeight =
            container.resolvedStyle.height;

        if (containerWidth <= 0f ||
            containerHeight <= 0f)
        {
            return;
        }

        float spriteWidth =
            mapSprite.rect.width;

        float spriteHeight =
            mapSprite.rect.height;

        if (spriteWidth <= 0f ||
            spriteHeight <= 0f)
        {
            return;
        }

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
            displayedHeight =
                displayedWidth / spriteAspect;

            imageOffsetX = 0f;
            imageOffsetY =
                (containerHeight - displayedHeight) * 0.5f;
        }
        else
        {
            displayedHeight = containerHeight;
            displayedWidth =
                displayedHeight * spriteAspect;

            imageOffsetX =
                (containerWidth - displayedWidth) * 0.5f;

            imageOffsetY = 0f;
        }

        float centerX =
            containerWidth * 0.5f;

        float centerY =
            containerHeight * 0.5f;

        foreach (var pair in fullMapMarkers)
        {
            MapMarkerTarget target = pair.Key;
            VisualElement marker = pair.Value;

            if (target == null || marker == null)
                continue;

            float normalizedX =
                Mathf.InverseLerp(
                    mapData.worldMin.x,
                    mapData.worldMax.x,
                    target.WorldPosition.x
                );

            float normalizedY =
                Mathf.InverseLerp(
                    mapData.worldMin.y,
                    mapData.worldMax.y,
                    target.WorldPosition.z
                );

            float markerX =
                imageOffsetX +
                normalizedX * displayedWidth;

            float markerY =
                imageOffsetY +
                (1f - normalizedY) * displayedHeight;

            markerX =
                centerX +
                (markerX - centerX) *
                mapUI.CurrentZoom +
                mapUI.MapOffset.x;

            markerY =
                centerY +
                (markerY - centerY) *
                mapUI.CurrentZoom +
                mapUI.MapOffset.y;

            float markerWidth =
                marker.resolvedStyle.width;

            float markerHeight =
                marker.resolvedStyle.height;

            marker.style.left =
                markerX - markerWidth * 0.5f;

            marker.style.top =
                markerY - markerHeight * 0.5f;
        }
    }
}