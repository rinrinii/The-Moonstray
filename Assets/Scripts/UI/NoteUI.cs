using System;
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
    private Action onCurrentNoteClosed;
    private bool isOpen;

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

        if (noteContentContainer != null)
        {
            noteContentContainer.verticalScrollerVisibility =
                ScrollerVisibility.Auto;
            noteContentContainer.horizontalScrollerVisibility =
                ScrollerVisibility.Hidden;
        }

        if (noteContent != null)
        {
            noteContent.style.whiteSpace = WhiteSpace.Normal;
            noteContent.style.unityTextAlign = TextAnchor.UpperLeft;
            noteContent.style.alignSelf = Align.Stretch;
        }

        closeButton =
            noteContainer.Q<Button>("CloseButton");

        if (closeButton != null)
        {
            closeButton.clicked -= CloseNote;
            closeButton.clicked += CloseNote;
        }

        CloseNote();
    }

    public void OpenNote(string title, string content, Action onClosed = null)
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
        onCurrentNoteClosed = onClosed;
        isOpen = true;

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

        bool wasOpen = isOpen;
        Action closedCallback = onCurrentNoteClosed;

        isOpen = false;
        onCurrentNoteClosed = null;

        noteContainer.style.display = DisplayStyle.None;
        noteContainer.pickingMode = PickingMode.Ignore;

        if (wasOpen)
            closedCallback?.Invoke();
    }
}
