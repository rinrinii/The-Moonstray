using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUIDocument : MonoBehaviour
{
    private VisualElement root;
    private VisualElement dialogueBox;
    private Label speakerName;
    private Label dialogueText;
    private VisualElement portraitImage;

    private void Awake()
    {
        UIDocument document = GetComponent<UIDocument>();

        root = document.rootVisualElement;

        dialogueBox = root.Q<VisualElement>("dialogue-box");
        speakerName = root.Q<Label>("speaker-name");
        dialogueText = root.Q<Label>("dialogue-text");
        portraitImage = root.Q<VisualElement>("portrait-image");

        Hide();
    }

    public void Display(DialogueLine line)
    {
        speakerName.text = line.speaker;
        dialogueText.text = line.text;

        if (!string.IsNullOrEmpty(line.portrait))
        {
            Texture2D portraitTexture =
                Resources.Load<Texture2D>($"Portraits/{line.portrait}");

            if (portraitTexture != null)
            {
                portraitImage.style.backgroundImage =
                    new StyleBackground(portraitTexture);
            }
        }
    }

    public void Show()
    {
        dialogueBox.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        dialogueBox.style.display = DisplayStyle.None;
    }
}