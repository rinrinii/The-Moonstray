using UnityEngine;

public class SubtleHighlightBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private void Awake()
    {
        if (visual != null)
            visual.SetActive(true);
    }
}