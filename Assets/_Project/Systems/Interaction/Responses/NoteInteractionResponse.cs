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
    private Action onReadCallback;

    public static event Action OnNoteRead;

    public void ConfigureOnRead(Action callback)
    {
        onReadCallback = callback;
    }

    private void Start()
    {
        ApplyCollectedStateIfNeeded();
    }

    public void OnInteract()
    {
        if (hasBeenRead)
            return;

        hasBeenRead = true;

        string title = GetNoteTitle();
        string content = GetNoteContent();

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

    private void ApplyCollectedStateIfNeeded()
    {
        if (!disableAfterPickup ||
            JournalController.Instance == null)
        {
            return;
        }

        string title = GetNoteTitle();

        if (string.IsNullOrWhiteSpace(title) ||
            !JournalController.Instance.HasNote(title))
        {
            return;
        }

        hasBeenRead = true;
        DisableNoteObject();
    }

    private void NotifyNoteRead()
    {
        onReadCallback?.Invoke();
        OnNoteRead?.Invoke();
    }

    private void DisableNoteObject()
    {
        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
            col.enabled = false;

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
            rend.enabled = false;

        Transform highlightAnchor =
            transform.Find("HighlightAnchor");

        if (highlightAnchor != null)
            highlightAnchor.gameObject.SetActive(false);
    }

    private string GetNoteTitle()
    {
        return noteData != null ? noteData.title : noteTitle;
    }

    private string GetNoteContent()
    {
        return noteData != null ? noteData.content : noteContent;
    }
}
