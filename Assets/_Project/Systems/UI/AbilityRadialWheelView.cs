using System;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class AbilityRadialWheelView : IDisposable
{
    private const string ResourcePath = "UI/AbilityRadialWheel";

    private readonly GameObject documentObject;
    private readonly VisualElement wheelRoot;
    private readonly VisualElement hudRoot;
    private readonly Label hudAbilityName;
    private readonly Label wheelSelectionName;
    private readonly VisualElement senseSegment;
    private readonly VisualElement howlSegment;
    private readonly VisualElement teleportSegment;

    private AbilityRadialWheelView(
        GameObject documentObject,
        VisualElement documentRoot)
    {
        this.documentObject = documentObject;
        wheelRoot = documentRoot.Q<VisualElement>("AbilityWheelRoot");
        hudRoot = documentRoot.Q<VisualElement>("EquippedAbilityHUD");
        hudAbilityName = documentRoot.Q<Label>("EquippedAbilityName");
        wheelSelectionName = documentRoot.Q<Label>("WheelSelectionName");
        senseSegment = documentRoot.Q<VisualElement>("SenseSegment");
        howlSegment = documentRoot.Q<VisualElement>("HowlSegment");
        teleportSegment = documentRoot.Q<VisualElement>("TeleportSegment");

        HideWheel();
    }

    public static AbilityRadialWheelView Create(Transform owner)
    {
        VisualTreeAsset template =
            Resources.Load<VisualTreeAsset>(ResourcePath);
        if (template == null)
            return null;

        PanelSettings panelSettings = FindPanelSettings();
        if (panelSettings == null)
            return null;

        GameObject documentObject = new("AbilityRadialWheelUI");
        documentObject.transform.SetParent(owner, false);

        UIDocument document = documentObject.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;
        document.sortingOrder = 100;
        document.visualTreeAsset = template;

        VisualElement root = document.rootVisualElement;
        if (root.Q<VisualElement>("AbilityWheelRoot") == null ||
            root.Q<VisualElement>("EquippedAbilityHUD") == null)
        {
            UnityEngine.Object.Destroy(documentObject);
            return null;
        }

        return new AbilityRadialWheelView(documentObject, root);
    }

    public void ShowWheel()
    {
        wheelRoot.style.display = DisplayStyle.Flex;
    }

    public void HideWheel()
    {
        wheelRoot.style.display = DisplayStyle.None;
        SetHighlightedAbility(string.Empty);
    }

    public void SetHudVisible(bool visible)
    {
        hudRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetEquippedAbility(string abilityName)
    {
        hudAbilityName.text = abilityName;
        hudRoot.EnableInClassList("ability-hud-hidden", string.IsNullOrEmpty(abilityName));
    }

    public void SetAbilityAvailable(string abilityName, bool available)
    {
        VisualElement segment = GetSegment(abilityName);
        if (segment != null)
            segment.style.display = available ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetHighlightedAbility(string abilityName)
    {
        senseSegment.EnableInClassList("ability-segment-selected", abilityName == "Sense");
        howlSegment.EnableInClassList("ability-segment-selected", abilityName == "Howl");
        teleportSegment.EnableInClassList("ability-segment-selected", abilityName == "Teleport");
        wheelSelectionName.text = string.IsNullOrEmpty(abilityName)
            ? "Release to cancel"
            : abilityName;
    }

    public void Dispose()
    {
        if (documentObject != null)
            UnityEngine.Object.Destroy(documentObject);
    }

    private VisualElement GetSegment(string abilityName)
    {
        return abilityName switch
        {
            "Sense" => senseSegment,
            "Howl" => howlSegment,
            "Teleport" => teleportSegment,
            _ => null
        };
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
