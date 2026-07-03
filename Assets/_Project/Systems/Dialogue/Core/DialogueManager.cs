using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance
    {
        get;
        private set;
    }

    private readonly Queue<DialogueLine> lines =
        new();

    private bool isActive;
    public event Action OnDialogueEnded;

    [Header("References")]
    [SerializeField]
    private DialogueUIDocument dialogueUI;

    [SerializeField]
    private DialogueDatabase database;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        FindDialogueUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded +=
            HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -=
            HandleSceneLoaded;
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        FindDialogueUI();
    }

    private void FindDialogueUI()
    {
        Debug.Log("Searching for Dialogue UI...");

        DialogueUIDocument[] docs =
            FindObjectsByType<DialogueUIDocument>(
                FindObjectsSortMode.None);

        Debug.Log($"Found {docs.Length} DialogueUIDocument(s)");

        foreach (var doc in docs)
        {
            Debug.Log($" -> {doc.name}");
        }

        dialogueUI = docs.Length > 0
            ? docs[0]
            : null;

        if (dialogueUI != null)
        {
            Debug.Log($"Assigned {dialogueUI.name}");
        }
    }

    public void StartDialogue(
        string dialogueID)
    {
        if (isActive)
        {
            Debug.LogWarning(
                "Dialogue already active."
            );

            return;
        }

        // Scene changed?
        if (dialogueUI == null)
        {
            FindDialogueUI();

            if (dialogueUI == null)
            {
                Debug.LogError(
                    "DialogueManager: Cannot start dialogue because DialogueUIDocument is missing."
                );

                return;
            }
        }

        if (database == null)
        {
            Debug.LogError(
                "DialogueManager: DialogueDatabase missing."
            );

            return;
        }

        List<DialogueLine> dialogueLines =
            database.GetDialogue(dialogueID);

        if (dialogueLines == null ||
            dialogueLines.Count == 0)
        {
            Debug.LogWarning(
                $"No dialogue found for ID: {dialogueID}"
            );

            return;
        }

        lines.Clear();

        foreach (DialogueLine line
                 in dialogueLines)
        {
            lines.Enqueue(line);
        }

        isActive = true;

        dialogueUI.ShowDialogueUI();

        DisplayNextLine();
    }

    public void StartDialogue(
    string dialogueID,
    Action onComplete)
    {
        void HandleFinished()
        {
            OnDialogueEnded -= HandleFinished;
            onComplete?.Invoke();
        }

        OnDialogueEnded += HandleFinished;

        StartDialogue(dialogueID);
    }

    public void DisplayNextLine()
    {
        if (!isActive)
            return;

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line =
            lines.Dequeue();

        dialogueUI.DisplayLine(line);

        dialogueUI.ShowNextArrow(
            lines.Count > 0
        );
    }

    public void EndDialogue()
    {
        isActive = false;

        if (dialogueUI != null)
        {
            dialogueUI.HideDialogueUI();
        }

        OnDialogueEnded?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return isActive;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextLine();
        }
    }
}