using UnityEngine;
using UnityEngine.UIElements;

public class PromptUI : MonoBehaviour
{
    private VisualElement promptHud;
    private VisualElement promptPanel;
    private Label promptHeader;
    private Label promptBody;
    private VisualElement promptKeyIcons;
    private VisualElement[] keyIcons;

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
        promptKeyIcons = root.Q<VisualElement>("PromptKeyIcons");
        keyIcons = new[]
        {
            root.Q<VisualElement>("PromptKeyE"),
            root.Q<VisualElement>("PromptKeyF"),
            root.Q<VisualElement>("PromptKeyI"),
            root.Q<VisualElement>("PromptKeyJ"),
            root.Q<VisualElement>("PromptKeyShift"),
            root.Q<VisualElement>("PromptKeySpace"),
            root.Q<VisualElement>("PromptKeyW"),
            root.Q<VisualElement>("PromptKeyA"),
            root.Q<VisualElement>("PromptKeyS"),
            root.Q<VisualElement>("PromptKeyD")
        };

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

        promptHeader.text = ConfigureKeyIcons(header);
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

        HideKeyIcons();

        if (promptPanel != null)
            promptPanel.style.display = DisplayStyle.None;

        if (promptHud != null)
            promptHud.style.display = DisplayStyle.None;
    }

    private string ConfigureKeyIcons(string header)
    {
        HideKeyIcons();

        if (string.IsNullOrWhiteSpace(header) || header[0] != '[')
            return header;

        int closingBracket = header.IndexOf(']');
        if (closingBracket <= 1)
            return header;

        string key = header.Substring(1, closingBracket - 1)
            .Trim()
            .ToUpperInvariant();

        bool found = key switch
        {
            "E" => ShowKey("PromptKeyE"),
            "F" => ShowKey("PromptKeyF"),
            "I" => ShowKey("PromptKeyI"),
            "J" => ShowKey("PromptKeyJ"),
            "SHIFT" or "LEFT SHIFT" => ShowKey("PromptKeyShift"),
            "SPACE" => ShowKey("PromptKeySpace"),
            "WASD" => ShowWasdKeys(),
            _ => false
        };

        if (!found)
            return header;

        return header[(closingBracket + 1)..].Trim();
    }

    private bool ShowWasdKeys()
    {
        ShowKey("PromptKeyW");
        ShowKey("PromptKeyA");
        ShowKey("PromptKeyS");
        ShowKey("PromptKeyD");
        return true;
    }

    private bool ShowKey(string elementName)
    {
        VisualElement icon = promptKeyIcons?.Q<VisualElement>(elementName);
        if (icon == null)
            return false;

        icon.style.display = DisplayStyle.Flex;
        promptKeyIcons.style.display = DisplayStyle.Flex;
        return true;
    }

    private void HideKeyIcons()
    {
        if (keyIcons != null)
        {
            foreach (VisualElement icon in keyIcons)
            {
                if (icon != null)
                    icon.style.display = DisplayStyle.None;
            }
        }

        if (promptKeyIcons != null)
            promptKeyIcons.style.display = DisplayStyle.None;
    }
}
