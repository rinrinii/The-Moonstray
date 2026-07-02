using UnityEngine;
using UnityEngine.UIElements;

public class ObjectivesUI : MonoBehaviour
{
    private VisualElement objectivesPanel;

    public void Initialize(VisualElement root)
    {
        objectivesPanel = root.Q<VisualElement>("ObjectivesPanel");

        Hide();
    }

    public void Show()
    {
        if (objectivesPanel == null)
            return;

        objectivesPanel.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (objectivesPanel == null)
            return;

        objectivesPanel.style.display = DisplayStyle.None;
    }
}