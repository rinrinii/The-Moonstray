using System;
using UnityEngine;

public class NoteInteractionResponse : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private NoteData noteData;

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

        string title = noteData != null ? noteData.title : noteTitle;
        string content = noteData != null ? noteData.content : noteContent;

        Debug.Log("NoteInteractionResponse fired: " + title);

        if (NoteUI.Instance != null)
        {
            NoteUI.Instance.OpenNote(title, content, NotifyNoteRead);
        }
        else
        {
            Debug.LogError("NoteUI.Instance missing");
            NotifyNoteRead();
        }

        if (JournalController.Instance != null)
        {
            JournalController.Instance.AddNote(title, content);
        }
        else
        {
            Debug.LogError("JournalController.Instance missing");
        }

        if (disableAfterPickup)
        {
            DisableNoteObject();
        }
    }

    private void NotifyNoteRead()
    {
        OnNoteRead?.Invoke();
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
