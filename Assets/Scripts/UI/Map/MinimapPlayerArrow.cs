using UnityEngine;
using UnityEngine.UIElements;

public class MinimapPlayerArrow : MonoBehaviour
{
    [SerializeField] private Transform player;

    private VisualElement minimapPlayerArrow;
    private VisualElement mapPlayerArrow;

    private void Start()
    {
        RefreshReferences();
    }

    private void Update()
    {
        if (player == null)
            return;

        if (minimapPlayerArrow == null ||
            mapPlayerArrow == null)
        {
            RefreshReferences();
        }

        float angle = player.eulerAngles.y;

        RotateArrow(minimapPlayerArrow, angle);
        RotateArrow(mapPlayerArrow, angle);
    }

    private void RefreshReferences()
    {
        VisualElement root =
            GameplayUIManager.Instance?.RootVisualElement;

        if (root == null)
            return;

        minimapPlayerArrow =
            root.Q<VisualElement>("MinimapPlayerArrow");

        mapPlayerArrow =
            root.Q<VisualElement>("MapPlayerArrow");
    }

    private void RotateArrow(
        VisualElement arrow,
        float angle)
    {
        if (arrow == null)
            return;

        arrow.style.rotate =
            new Rotate(
                new Angle(angle, AngleUnit.Degree)
            );
    }
}