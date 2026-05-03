using UnityEngine;

public class ToggleObject : MonoBehaviour, IInteractable, IHighlightable
{
    public GameObject target;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void Interact()
    {
        target.SetActive(!target.activeSelf);
        Debug.Log("Toggled object: " + target.name);
    }

    public void Highlight()
    {
        rend.material.color = Color.cyan;
    }

    public void Unhighlight()
    {
        rend.material.color = originalColor;
    }
}