using UnityEngine;

public class ExplorationHighlightBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject visual;

    private void Awake()
    {
        if (visual != null)
            visual.SetActive(true);
    }
}