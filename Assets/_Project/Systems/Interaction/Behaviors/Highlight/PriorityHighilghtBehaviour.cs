using UnityEngine;

public class PriorityHighlightBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject marker;

    private void Awake()
    {
        if (marker != null)
            marker.SetActive(true);
    }
}