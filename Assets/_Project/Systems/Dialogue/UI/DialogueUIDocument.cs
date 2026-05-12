using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUIDocument : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement dialogueBackground;
    private VisualElement portraitImage;
    private VisualElement nextArrow;
    private VisualElement nameBox;

    private Label speakerName;
    private Label dialogueText;

    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();

        VisualElement root =
            uiDocument.rootVisualElement;

        dialogueBackground =
            root.Q<VisualElement>("DialogueContainer");

        portraitImage =
            root.Q<VisualElement>("portrait-image");

        nextArrow =
            root.Q<VisualElement>("next-arrow");

        nameBox =
            root.Q<VisualElement>("name-box");

        speakerName =
            root.Q<Label>("speaker-name");

        dialogueText =
            root.Q<Label>("dialogue-text");

        HideDialogueUI();
    }

    public void DisplayLine(DialogueLine line)
    {
        dialogueText.text =
            line.text;

        if (line.isNarration)
        {
            speakerName.text = "";

            nameBox.style.display =
                DisplayStyle.None;

            portraitImage.style.display =
                DisplayStyle.None;

            dialogueText.style.unityFontStyleAndWeight =
                FontStyle.Italic;
        }
        else
        {
            speakerName.text =
                line.speaker;

            nameBox.style.display =
                DisplayStyle.Flex;

            portraitImage.style.display =
                DisplayStyle.Flex;

            dialogueText.style.unityFontStyleAndWeight =
                FontStyle.Normal;

            if (line.portrait != null)
            {
                portraitImage.style.backgroundImage =
                    new StyleBackground(line.portrait);
            }
        }
    }

    public void ShowDialogueUI()
    {
        dialogueBackground.style.display =
            DisplayStyle.Flex;
    }

    public void HideDialogueUI()
    {
        dialogueBackground.style.display =
            DisplayStyle.None;
    }

    public void ShowNextArrow(bool show)
    {
        nextArrow.style.display =
            show
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }
}