using UnityEngine;
using UnityEngine.UIElements;

public class PromptUI : MonoBehaviour
{
    private VisualElement promptHud;
    private VisualElement promptPanel;
    private Label promptHeader;
    private Label promptBody;

    public static PromptUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize(VisualElement root)
    {
        promptHud = root.Q<VisualElement>("BottomCenterPromptHUD");
        promptPanel = root.Q<VisualElement>("PromptPanel");
        promptHeader = root.Q<Label>("PromptHeader");
        promptBody = root.Q<Label>("PromptBody");

        Hide();
    }

    public void Show(string header, string body)
    {
        if (promptHud == null ||
            promptPanel == null ||
            promptHeader == null ||
            promptBody == null)
            return;

        if (string.IsNullOrWhiteSpace(header) &&
            string.IsNullOrWhiteSpace(body))
        {
            Hide();
            return;
        }

        promptHeader.text = header;
        promptBody.text = body;

        promptHud.style.display = DisplayStyle.Flex;
        promptPanel.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (promptHeader != null)
            promptHeader.text = string.Empty;

        if (promptBody != null)
            promptBody.text = string.Empty;

        if (promptPanel != null)
            promptPanel.style.display = DisplayStyle.None;

        if (promptHud != null)
            promptHud.style.display = DisplayStyle.None;
    }
}
