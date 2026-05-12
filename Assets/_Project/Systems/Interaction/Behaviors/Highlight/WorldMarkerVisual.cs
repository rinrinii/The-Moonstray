using UnityEngine;

public class WorldMarkerVisual : MonoBehaviour, IHighlightVisual
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}