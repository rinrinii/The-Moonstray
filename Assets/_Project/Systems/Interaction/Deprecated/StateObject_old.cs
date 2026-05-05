using UnityEngine;

public class StateObject : MonoBehaviour, IInteractable, IHighlightable
{
    private Renderer rend;
    private Color originalColor;
    private bool isActive = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void Interact()
    {
        isActive = !isActive;

        rend.material.color = isActive ? Color.green : originalColor;

        Debug.Log("State changed: " + isActive);
    }

    public void Highlight()
    {
        rend.material.color = Color.yellow;
    }

    public void Unhighlight()
    {
        if (!isActive)
            rend.material.color = originalColor;
        else
            rend.material.color = Color.green;
    }
}