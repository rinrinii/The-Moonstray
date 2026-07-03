using UnityEngine;

public class ObjectStateHighlightMarker : MonoBehaviour
{
    [Header("Object State")]
    [SerializeField] private string objectID;

    [Tooltip("The object state in which the highlight marker should be visible.")]
    [SerializeField] private int visibleState = 0;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightAnchor;

    private void Reset()
    {
        Transform anchor = transform.Find("HighlightAnchor");

        if (anchor != null)
            highlightAnchor = anchor.gameObject;
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnObjectStateChanged += HandleStateChanged;
            Debug.Log($"{name}: Subscribed.");
        }
        else
        {
            Debug.LogWarning($"{name}: GameStateManager missing during Start.");
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnObjectStateChanged -= HandleStateChanged;
    }

    public void Refresh()
    {
        if (highlightAnchor == null)
        {
            Debug.LogWarning($"{name}: Highlight Anchor missing.");
            return;
        }

        if (string.IsNullOrEmpty(objectID))
        {
            Debug.LogWarning($"{name}: Object ID missing.");
            return;
        }

        if (GameStateManager.Instance == null)
            return;

        int currentState = GameStateManager.Instance.GetState(objectID);

        bool shouldShow = currentState == visibleState;

        Debug.Log($"{name}: Refresh ({currentState}) -> {shouldShow}");

        highlightAnchor.SetActive(shouldShow);
    }

    private void HandleStateChanged(string changedObjectID, int newState)
    {
        Debug.Log($"{name}: Event {changedObjectID} -> {newState}");

        if (changedObjectID != objectID)
            return;

        Refresh();
    }
}