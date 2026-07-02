using UnityEngine;
using UnityEngine.UIElements;

public class PromptUI : MonoBehaviour
{
    private VisualElement promptPanel;
    private Label promptHeader;
    private Label promptBody;

    public void Initialize(VisualElement root)
    {
        promptPanel = root.Q<VisualElement>("PromptPanel");
        promptHeader = root.Q<Label>("PromptHeader");
        promptBody = root.Q<Label>("PromptBody");

        Hide();
    }

    public void Show(string header, string body)
    {
        if (promptPanel == null ||
            promptHeader == null ||
            promptBody == null)
            return;

        promptHeader.text = header;
        promptBody.text = body;

        promptPanel.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (promptPanel == null)
            return;

        promptHeader.text = string.Empty;
        promptBody.text = string.Empty;

        promptPanel.style.display = DisplayStyle.None;
    }
}