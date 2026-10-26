using System;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class CutsceneSkipPromptView : IDisposable
{
    private const string ResourcePath = "UI/CutsceneSkipPrompt";

    private readonly GameObject documentObject;
    private readonly PanelSettings panelSettings;
    private readonly VisualElement root;
    private readonly Button yesButton;
    private readonly Button noButton;

    private Action onYes;
    private Action onNo;

    private CutsceneSkipPromptView(
        GameObject documentObject,
        PanelSettings panelSettings,
        VisualElement root,
        Button yesButton,
        Button noButton)
    {
        this.documentObject = documentObject;
        this.panelSettings = panelSettings;
        this.root = root;
        this.yesButton = yesButton;
        this.noButton = noButton;

        yesButton.clicked += HandleYes;
        noButton.clicked += HandleNo;
        root.style.display = DisplayStyle.None;
    }

    public static CutsceneSkipPromptView Create(Transform owner)
    {
        VisualTreeAsset template =
            Resources.Load<VisualTreeAsset>(ResourcePath);
        if (template == null)
        {
            Debug.LogError(
                $"Cutscene skip prompt is missing at Resources/{ResourcePath}.uxml.");
            return null;
        }

        PanelSettings sourcePanelSettings = FindPanelSettings();
        if (sourcePanelSettings == null)
        {
            Debug.LogError(
                "Cutscene skip prompt could not find UI Toolkit PanelSettings.");
            return null;
        }

        PanelSettings promptPanelSettings =
            UnityEngine.Object.Instantiate(sourcePanelSettings);
        promptPanelSettings.name = "Cutscene Skip Prompt Panel Settings";
        promptPanelSettings.sortingOrder = 2000;

        GameObject documentObject = new("CutsceneSkipPrompt");
        documentObject.transform.SetParent(owner, false);

        UIDocument document = documentObject.AddComponent<UIDocument>();
        document.panelSettings = promptPanelSettings;
        document.sortingOrder = 0;
        document.visualTreeAsset = template;

        VisualElement root =
            document.rootVisualElement.Q<VisualElement>("CutsceneSkipPrompt");
        Button yesButton =
            document.rootVisualElement.Q<Button>("SkipYesButton");
        Button noButton =
            document.rootVisualElement.Q<Button>("SkipNoButton");

        if (root == null || yesButton == null || noButton == null)
        {
            Debug.LogError(
                "Cutscene skip prompt is missing required named elements.");
            UnityEngine.Object.Destroy(documentObject);
            UnityEngine.Object.Destroy(promptPanelSettings);
            return null;
        }

        return new CutsceneSkipPromptView(
            documentObject,
            promptPanelSettings,
            root,
            yesButton,
            noButton);
    }

    public void Show(Action yes, Action no)
    {
        onYes = yes;
        onNo = no;
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
        onYes = null;
        onNo = null;
    }

    public void SetButtonsEnabled(bool enabled)
    {
        yesButton.SetEnabled(enabled);
        noButton.SetEnabled(enabled);
    }

    public void Dispose()
    {
        yesButton.clicked -= HandleYes;
        noButton.clicked -= HandleNo;
        onYes = null;
        onNo = null;

        if (documentObject != null)
            UnityEngine.Object.Destroy(documentObject);
        if (panelSettings != null)
            UnityEngine.Object.Destroy(panelSettings);
    }

    private void HandleYes()
    {
        onYes?.Invoke();
    }

    private void HandleNo()
    {
        onNo?.Invoke();
    }

    private static PanelSettings FindPanelSettings()
    {
        UIDocument[] documents =
            UnityEngine.Object.FindObjectsByType<UIDocument>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (UIDocument document in documents)
        {
            if (document != null && document.panelSettings != null)
                return document.panelSettings;
        }

        return null;
    }
}
