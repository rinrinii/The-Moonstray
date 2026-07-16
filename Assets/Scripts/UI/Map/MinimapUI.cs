using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MinimapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private MapData[] sceneMaps;

    [Header("Minimap")]
    [SerializeField] private float minimapZoom = 3f;

    private VisualElement minimapContainer;
    private Image minimapImage;
    private VisualElement playerArrow;

    private MapData currentMap;

    public MapData CurrentMap => currentMap;
    public VisualElement MinimapContainer => minimapContainer;
    public Image MinimapImage => minimapImage;

    private void Start()
    {
        RefreshReferences();
        LoadSceneMinimap();
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
        RefreshReferences();
        LoadSceneMinimap();
    }

    private void LateUpdate()
    {
        UpdateMinimapPosition();
    }

    private void RefreshReferences()
    {
        VisualElement root =
            GameplayUIManager.Instance?.RootVisualElement;

        if (root == null)
            return;

        minimapContainer =
            root.Q<VisualElement>("MinimapBorder");

        minimapImage =
            root.Q<Image>("MinimapImage");

        playerArrow =
            root.Q<VisualElement>("MinimapPlayerArrow");
    }

    private void LoadSceneMinimap()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        currentMap = FindSceneMap(sceneName);

        if (currentMap == null ||
            currentMap.mapSprite == null)
        {
            if (minimapImage != null)
                minimapImage.image = null;

            Debug.LogWarning(
                $"MinimapUI: No map assigned for scene '{sceneName}'."
            );

            return;
        }

        if (minimapImage == null)
            return;

        minimapImage.image =
            currentMap.mapSprite.texture;

        minimapImage.scaleMode =
            ScaleMode.StretchToFill;

        minimapImage.style.transformOrigin =
            new TransformOrigin(
                Length.Percent(0),
                Length.Percent(0),
                0
            );
    }

    private void UpdateMinimapPosition()
    {
        if (player == null ||
            currentMap == null ||
            currentMap.mapSprite == null ||
            minimapContainer == null ||
            minimapImage == null ||
            playerArrow == null)
        {
            return;
        }

        float containerWidth =
            minimapContainer.resolvedStyle.width;

        float containerHeight =
            minimapContainer.resolvedStyle.height;

        if (containerWidth <= 0f ||
            containerHeight <= 0f)
        {
            return;
        }

        float spriteWidth =
            currentMap.mapSprite.rect.width;

        float spriteHeight =
            currentMap.mapSprite.rect.height;

        if (spriteWidth <= 0f ||
            spriteHeight <= 0f)
        {
            return;
        }

        float spriteAspect =
            spriteWidth / spriteHeight;

        float containerAspect =
            containerWidth / containerHeight;

        float baseWidth;
        float baseHeight;

        if (spriteAspect > containerAspect)
        {
            baseHeight = containerHeight;
            baseWidth = baseHeight * spriteAspect;
        }
        else
        {
            baseWidth = containerWidth;
            baseHeight = baseWidth / spriteAspect;
        }

        float scaledWidth =
            baseWidth * minimapZoom;

        float scaledHeight =
            baseHeight * minimapZoom;

        minimapImage.style.width =
            scaledWidth;

        minimapImage.style.height =
            scaledHeight;

        minimapImage.style.scale =
            new Scale(Vector2.one);

        float normalizedX =
            Mathf.InverseLerp(
                currentMap.worldMin.x,
                currentMap.worldMax.x,
                player.position.x
            );

        float normalizedY =
            Mathf.InverseLerp(
                currentMap.worldMin.y,
                currentMap.worldMax.y,
                player.position.z
            );

        float playerMapX =
            normalizedX * scaledWidth;

        float playerMapY =
            (1f - normalizedY) * scaledHeight;

        float desiredX =
            containerWidth * 0.5f -
            playerMapX;

        float desiredY =
            containerHeight * 0.5f -
            playerMapY;

        float minimumX =
            containerWidth - scaledWidth;

        float minimumY =
            containerHeight - scaledHeight;

        float clampedX =
            Mathf.Clamp(
                desiredX,
                minimumX,
                0f
            );

        float clampedY =
            Mathf.Clamp(
                desiredY,
                minimumY,
                0f
            );

        minimapImage.style.translate =
            new Translate(
                clampedX,
                clampedY,
                0
            );

        float arrowX =
            playerMapX + clampedX;

        float arrowY =
            playerMapY + clampedY;

        playerArrow.style.left =
            arrowX -
            playerArrow.resolvedStyle.width * 0.5f;

        playerArrow.style.top =
            arrowY -
            playerArrow.resolvedStyle.height * 0.5f;
    }

    private MapData FindSceneMap(string sceneName)
    {
        if (sceneMaps != null)
        {
            foreach (MapData map in sceneMaps)
            {
                if (map != null &&
                    map.sceneName == sceneName)
                {
                    return map;
                }
            }
        }

        MapData[] resourceMaps =
            Resources.LoadAll<MapData>("Map");

        foreach (MapData map in resourceMaps)
        {
            if (map != null &&
                map.sceneName == sceneName)
            {
                return map;
            }
        }

        return null;
    }
}