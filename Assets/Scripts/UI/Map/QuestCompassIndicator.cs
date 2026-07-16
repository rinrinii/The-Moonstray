using UnityEngine;
using UnityEngine.UIElements;

public class QuestCompassIndicator : MonoBehaviour
{
    public static QuestCompassIndicator Instance { get; private set; }

    [SerializeField] private Transform player;
    private Transform activeQuestTarget;
    [SerializeField] private float edgeRadius = 0.56f;
    [SerializeField] private float rotationOffset;

    private VisualElement minimapContainer;
    private VisualElement compassArrow;

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

    private void Start()
    {
        RefreshReferences();
    }

    private void LateUpdate()
    {
        UpdateIndicator();
    }

    private void RefreshReferences()
    {
        VisualElement root =
            GameplayUIManager.Instance?.RootVisualElement;

        if (root == null)
            return;

        minimapContainer =
            root.Q<VisualElement>("MinimapWrapper");

        compassArrow =
            root.Q<VisualElement>("QuestCompassArrow");
    }

    private void UpdateIndicator()
    {
        if (minimapContainer == null ||
            compassArrow == null)
        {
            RefreshReferences();

            if (minimapContainer == null ||
                compassArrow == null)
            {
                return;
            }
        }

        if (player == null ||
            activeQuestTarget == null)
        {
            compassArrow.style.display =
                DisplayStyle.None;

            return;
        }

        float width =
            minimapContainer.resolvedStyle.width;

        float height =
            minimapContainer.resolvedStyle.height;

        if (width <= 0f ||
            height <= 0f)
        {
            return;
        }

        Vector3 direction =
            activeQuestTarget.position -
            player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            compassArrow.style.display =
                DisplayStyle.None;

            return;
        }

        compassArrow.style.display =
            DisplayStyle.Flex;

        float angle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg +
            rotationOffset;

        float centerX =
            width * 0.5f;

        float centerY =
            height * 0.5f;

        float radius =
            Mathf.Min(width, height) *
            edgeRadius;

        float radians =
            angle * Mathf.Deg2Rad;

        float arrowX =
            centerX +
            Mathf.Sin(radians) *
            radius;

        float arrowY =
            centerY -
            Mathf.Cos(radians) *
            radius;

        float arrowWidth =
            compassArrow.resolvedStyle.width;

        float arrowHeight =
            compassArrow.resolvedStyle.height;

        compassArrow.style.left =
            arrowX -
            arrowWidth * 0.5f;

        compassArrow.style.top =
            arrowY -
            arrowHeight * 0.5f;

        compassArrow.style.transformOrigin =
            new TransformOrigin(
                Length.Percent(50),
                Length.Percent(50),
                0
            );

        compassArrow.style.rotate =
            new Rotate(
                new Angle(
                    angle,
                    AngleUnit.Degree
                )
            );
    }

    public void SetActiveQuestTarget(
        Transform target)
    {
        activeQuestTarget = target;
    }

    public void ClearActiveQuestTarget()
    {
        activeQuestTarget = null;
    }
}