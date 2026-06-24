using UnityEngine;
using UnityEngine.UIElements;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    private VisualElement noteContainer;
    private Label noteTitle;
    private Label noteContent;
    private Button closeButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        var ui = GameplayUIManager.Instance;
        noteContainer = ui.NotePopupRoot;

        if (noteContainer == null) return;

        noteTitle = noteContainer.Q<Label>("NoteTitle");
        noteContent = noteContainer.Q<Label>("NoteContent");
        closeButton = noteContainer.Q<Button>("CloseButton");

        if (closeButton != null)
            closeButton.clicked += CloseNote;

        CloseNote();
    }

    public void OpenNote(string title, string content)
    {
        Debug.Log("OpenNote called: " + title);

        if (noteContainer == null)
        {
            Debug.LogError("NoteContainer missing");
            return;
        }

        if (noteTitle != null) noteTitle.text = title;
        if (noteContent != null) noteContent.text = content;

        noteContainer.style.display = DisplayStyle.Flex;
        noteContainer.pickingMode = PickingMode.Position;
    }

    public void CloseNote()
    {
        if (noteContainer == null) return;

        noteContainer.style.display = DisplayStyle.None;
        noteContainer.pickingMode = PickingMode.Ignore;
    }
}