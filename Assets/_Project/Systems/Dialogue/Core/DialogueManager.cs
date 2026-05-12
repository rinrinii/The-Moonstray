using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();
    private bool isActive = false;

    [Header("References")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private DialogueDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDialogue(string dialogueID)
    {
        // prevent overlapping dialogues
        if (isActive)
        {
            Debug.LogWarning("Dialogue already active.");
            return;
        }

        List<DialogueLine> dialogueLines = database.GetDialogue(dialogueID);

        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning($"No dialogue lines found for ID: {dialogueID}");
            return;
        }

        lines.Clear();

        foreach (DialogueLine line in dialogueLines)
        {
            lines.Enqueue(line);
        }

        isActive = true;

        dialogueUI.Show();

        DisplayNextLine();
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

        DialogueLine line = lines.Dequeue();

        dialogueUI.Display(line);
    }

    public void EndDialogue()
    {
        isActive = false;

        dialogueUI.Hide();
    }

    public bool IsDialogueActive()
    {
        return isActive;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextLine();
        }
    }
}