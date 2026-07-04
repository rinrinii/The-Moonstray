using System;
using UnityEngine;

public class NoteInteractionResponse : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private string noteTitle;

    [TextArea(5, 15)]
    [SerializeField] private string noteContent;

    [SerializeField] private bool disableAfterPickup = true;

    public static event Action OnNoteRead;

    public void OnInteract()
    {
        Debug.Log("NoteInteractionResponse fired: " + noteTitle);

        if (NoteUI.Instance != null)
            NoteUI.Instance.OpenNote(noteTitle, noteContent);
        else
            Debug.LogError("NoteUI.Instance missing");

        if (JournalController.Instance != null)
            JournalController.Instance.AddNote(noteTitle, noteContent);
        else
            Debug.LogError("JournalController.Instance missing");

        if (disableAfterPickup)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
        }

        OnNoteRead?.Invoke();
    }
}