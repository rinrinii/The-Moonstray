using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    private VisualElement noteContainer;
    private ScrollView noteContentContainer;
    private Label noteTitle;
    private Label noteContent;
    private Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        var ui = GameplayUIManager.Instance;

        if (ui == null)
            return;

        noteContainer = ui.NotePopupRoot;

        if (noteContainer == null)
            return;

        noteContentContainer =
            noteContainer.Q<ScrollView>("NoteContentContainer");

        noteTitle =
            noteContainer.Q<Label>("NoteTitle");

        noteContent =
            noteContainer.Q<Label>("NoteContent");

        closeButton =
            noteContainer.Q<Button>("CloseButton");

        if (closeButton != null)
        {
            closeButton.clicked -= CloseNote;
            closeButton.clicked += CloseNote;
        }

        CloseNote();
    }

    public void OpenNote(string title, string content)
    {
        if (noteContainer == null ||
            noteTitle == null ||
            noteContent == null)
        {
            RefreshReferences();
        }

        if (noteContainer == null ||
            noteTitle == null ||
            noteContent == null)
        {
            Debug.LogError("Note UI references missing.");
            return;
        }

        noteTitle.text = title;
        noteContent.text = content;

        noteContainer.style.display = DisplayStyle.Flex;
        noteContainer.pickingMode = PickingMode.Position;

        if (noteContentContainer != null)
        {
            noteContentContainer.scrollOffset = Vector2.zero;
        }
    }

    public void CloseNote()
    {
        if (noteContainer == null)
            return;

        noteContainer.style.display = DisplayStyle.None;
        noteContainer.pickingMode = PickingMode.Ignore;
    }
}