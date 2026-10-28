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

    private IReadOnlyList<DialogueChoice> activeChoices;

    private bool isActive;
    private bool isChoosing;
    private bool waitForSpaceRelease;

    private string currentDialogueID;
    public string CurrentDialogueID => currentDialogueID;

    public event Action<string> OnDialogueEnded;

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

        currentDialogueID = dialogueID;
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

    public void ShowPrompt(
        string prompt,
        IReadOnlyList<DialogueChoice> choices)
    {
        if (isActive)
        {
            Debug.LogWarning("Dialogue already active.");
            return;
        }

        if (dialogueUI == null)
            FindDialogueUI();

        if (dialogueUI == null || choices == null || choices.Count == 0)
        {
            Debug.LogError("DialogueManager: Cannot show a prompt without UI and choices.");
            return;
        }

        currentDialogueID = string.Empty;
        lines.Clear();
        activeChoices = choices;
        isActive = true;
        isChoosing = true;

        dialogueUI.ShowDialogueUI();
        dialogueUI.DisplayPrompt(prompt, choices, SelectChoice);
    }

    public void ShowPrompt(
        string prompt,
        string speaker,
        string portraitName,
        IReadOnlyList<DialogueChoice> choices)
    {
        if (isActive)
        {
            Debug.LogWarning("Dialogue already active.");
            return;
        }

        if (dialogueUI == null)
            FindDialogueUI();

        if (dialogueUI == null || choices == null || choices.Count == 0)
        {
            Debug.LogError("DialogueManager: Cannot show a prompt without UI and choices.");
            return;
        }

        if (portraitName == "serin-current")
        {
            portraitName =
                PlayerTransformation.Instance != null &&
                PlayerTransformation.Instance.IsWolf
                    ? "serin-wolf"
                    : "serin-human";
        }

        Sprite portrait = string.IsNullOrWhiteSpace(portraitName)
            ? null
            : Resources.Load<Sprite>($"Portraits/{portraitName}");

        currentDialogueID = string.Empty;
        lines.Clear();
        activeChoices = choices;
        isActive = true;
        isChoosing = true;

        dialogueUI.ShowDialogueUI();
        dialogueUI.DisplayInlineChoice(
            new DialogueLine
            {
                speaker = speaker,
                text = prompt,
                portrait = portrait
            },
            choices,
            SelectChoice);
    }

    private void SelectChoice(int index)
    {
        if (!isChoosing)
            return;

        if (activeChoices == null || index < 0 || index >= activeChoices.Count)
            return;

        DialogueChoice selectedChoice = activeChoices[index];
        isChoosing = false;
        activeChoices = null;
        EndDialogue();

        // EndDialogue clears UI state before the selected action changes scenes.
        selectedChoice.Select();
    }

    private void SelectInlineChoice(int index)
    {
        if (!isChoosing || activeChoices == null ||
            index < 0 || index >= activeChoices.Count)
        {
            return;
        }

        isChoosing = false;
        activeChoices = null;
        dialogueUI.ClearChoices();
        DisplayNextLine();
    }

    public void StartDialogue(
    string dialogueID,
    Action onComplete)
    {
        void HandleFinished(string finishedDialogueID)
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

        if (line.HasChoices)
        {
            List<DialogueChoice> choices = new();
            foreach (string choiceText in line.choices)
                choices.Add(new DialogueChoice(choiceText, null));

            activeChoices = choices;
            isChoosing = true;
            dialogueUI.DisplayInlineChoice(
                line,
                choices,
                SelectInlineChoice);
            return;
        }

        dialogueUI.DisplayLine(line);

        dialogueUI.ShowNextArrow(
            lines.Count > 0
        );
    }

    public void EndDialogue()
    {
        isActive = false;
        isChoosing = false;
        activeChoices = null;

        // Don't let gameplay receive the same Space press
        waitForSpaceRelease = true;

        if (dialogueUI != null)
        {
            dialogueUI.HideDialogueUI();
        }

        OnDialogueEnded?.Invoke(currentDialogueID);
        currentDialogueID = string.Empty;
    }

    public bool IsGameplayInputBlocked()
    {
        return waitForSpaceRelease;
    }

    public bool IsDialogueActive => isActive;

    private void Update()
    {
        if (waitForSpaceRelease)
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                waitForSpaceRelease = false;
            }

            return;
        }

        if (!isActive || isChoosing)
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
