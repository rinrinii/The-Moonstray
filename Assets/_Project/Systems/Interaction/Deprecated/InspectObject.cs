using UnityEngine;

public class InspectObject : MonoBehaviour, IInteractable, IHighlightable
{
    public string message = "You inspected something.";

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void Interact()
    {
        Debug.Log(message);
    }

    public void Highlight()
    {
        rend.material.color = Color.yellow;
    }

    public void Unhighlight()
    {
        rend.material.color = originalColor;
    }
}