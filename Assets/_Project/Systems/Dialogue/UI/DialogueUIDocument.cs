using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUIDocument : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement dialogueBackground;
    private VisualElement portraitImage;
    private VisualElement nextArrow;
    private VisualElement nameBox;
    private VisualElement choicesContainer;

    // HUD reference
    private VisualElement hudContainer;

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

        choicesContainer =
            root.Q<VisualElement>("DialogueChoices");

        // HUD Container
        hudContainer =
            root.Q<VisualElement>("HUDContainer");

        speakerName =
            root.Q<Label>("speaker-name");

        dialogueText =
            root.Q<Label>("dialogue-text");

        HideDialogueUI();
    }

    public void DisplayLine(DialogueLine line)
    {
        ClearChoices();

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
            dialogueText.style.top = 60;
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
            dialogueText.style.top = 80;

            if (line.portrait != null)
            {
                portraitImage.style.backgroundImage =
                    new StyleBackground(line.portrait);
            }
        }
    }

    public void DisplayPrompt(
        string prompt,
        IReadOnlyList<DialogueChoice> choices,
        Action<int> onSelected)
    {
        dialogueText.text = prompt;
        dialogueText.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
        dialogueText.style.top = 60;

        speakerName.text = "";
        nameBox.style.display = DisplayStyle.None;
        portraitImage.style.display = DisplayStyle.None;
        nextArrow.style.display = DisplayStyle.None;

        ClearChoices();
        if (choicesContainer == null || choices == null)
            return;

        for (int index = 0; index < choices.Count; index++)
        {
            int selectedIndex = index;
            Button button = new(() => onSelected?.Invoke(selectedIndex))
            {
                text = choices[index].Text
            };

            button.AddToClassList("dialogue-choice-button");
            choicesContainer.Add(button);
        }

        choicesContainer.style.display = DisplayStyle.Flex;
    }

    public void DisplayInlineChoice(
        DialogueLine line,
        IReadOnlyList<DialogueChoice> choices,
        Action<int> onSelected)
    {
        DisplayLine(line);
        dialogueText.style.unityFontStyleAndWeight = FontStyle.Italic;
        nextArrow.style.display = DisplayStyle.None;

        PopulateChoices(choices, onSelected);
    }

    private void PopulateChoices(
        IReadOnlyList<DialogueChoice> choices,
        Action<int> onSelected)
    {
        ClearChoices();
        if (choicesContainer == null || choices == null)
            return;

        for (int index = 0; index < choices.Count; index++)
        {
            int selectedIndex = index;
            Button button = new(() => onSelected?.Invoke(selectedIndex))
            {
                text = choices[index].Text
            };

            button.AddToClassList("dialogue-choice-button");
            choicesContainer.Add(button);
        }

        choicesContainer.style.display = DisplayStyle.Flex;
    }

    public void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        choicesContainer.Clear();
        choicesContainer.style.display = DisplayStyle.None;
    }

    public void ShowDialogueUI()
    {
        // Show dialogue UI
        dialogueBackground.style.display =
            DisplayStyle.Flex;

        // Hide HUD during dialogue
        if (hudContainer != null)
        {
            hudContainer.style.display =
                DisplayStyle.None;
        }
    }

    public void HideDialogueUI()
    {
        ClearChoices();

        // Hide dialogue UI
        dialogueBackground.style.display =
            DisplayStyle.None;

        // Show HUD again
        if (hudContainer != null)
        {
            GameplayUIManager gameplayUI =
                GameplayUIManager.Instance;

            if (gameplayUI != null)
            {
                gameplayUI.ApplyCaptureHudVisibility();
            }
            else
            {
                hudContainer.style.display =
                    GameplayUIManager.IsCaptureHudHidden
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;
            }
        }
    }

    public void ShowNextArrow(bool show)
    {
        nextArrow.style.display =
            show
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }
}
