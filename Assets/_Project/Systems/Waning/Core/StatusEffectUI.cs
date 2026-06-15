using UnityEngine;
using UnityEngine.UIElements;

public class StatusEffectUI : MonoBehaviour
{
    private VisualElement thornIcon;
    private VisualElement slowIcon;
    private VisualElement frostbiteIcon;
    private VisualElement poisonIcon;

    private void Start()
    {
        var root = GameplayUIManager.Instance.RootVisualElement;

        thornIcon = root.Q<VisualElement>("ThornIcon");
        slowIcon = root.Q<VisualElement>("SlowIcon");
        frostbiteIcon = root.Q<VisualElement>("FrostbiteIcon");
        poisonIcon = root.Q<VisualElement>("PoisonIcon");

        SetIcon(thornIcon, false);
        SetIcon(slowIcon, false);
        SetIcon(frostbiteIcon, false);
        SetIcon(poisonIcon, false);

        if (StatusEffectManager.Instance != null)
            StatusEffectManager.Instance.OnStatusChanged += RefreshIcons;
    }

    private void OnDestroy()
    {
        if (StatusEffectManager.Instance != null)
            StatusEffectManager.Instance.OnStatusChanged -= RefreshIcons;
    }

    private void RefreshIcons()
    {
        var mgr = StatusEffectManager.Instance;
        if (mgr == null) return;

        SetIcon(thornIcon, mgr.HasThorn);
        SetIcon(slowIcon, mgr.HasSlow);
        SetIcon(frostbiteIcon, mgr.HasFrostbite);
        SetIcon(poisonIcon, mgr.HasPoison);
    }

    private void SetIcon(VisualElement icon, bool visible)
    {
        if (icon == null) return;
        icon.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }
}