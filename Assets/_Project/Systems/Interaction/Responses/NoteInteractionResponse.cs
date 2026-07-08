using System;
using UnityEngine;

public class NoteInteractionResponse : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string noteTitle;

    [TextArea(5, 15)]
    [SerializeField] private string noteContent;

    [SerializeField] private bool disableAfterPickup = true;

    private bool hasBeenRead;

    public static event Action OnNoteRead;

    public void OnInteract()
    {
        if (hasBeenRead)
            return;

        hasBeenRead = true;

        Debug.Log("NoteInteractionResponse fired: " + noteTitle);

        if (NoteUI.Instance != null)
        {
            NoteUI.Instance.OpenNote(noteTitle, noteContent);
        }
        else
        {
            Debug.LogError("NoteUI.Instance missing");
        }

        if (JournalController.Instance != null)
        {
            JournalController.Instance.AddNote(noteTitle, noteContent);
        }
        else
        {
            Debug.LogError("JournalController.Instance missing");
        }

        OnNoteRead?.Invoke();

        if (disableAfterPickup)
        {
            DisableNoteObject();
        }
    }

    private void DisableNoteObject()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        Renderer rend = GetComponent<Renderer>();

        if (rend != null)
            rend.enabled = false;

        Transform highlightAnchor =
            transform.Find("HighlightAnchor");

        if (highlightAnchor != null)
            highlightAnchor.gameObject.SetActive(false);
    }
}