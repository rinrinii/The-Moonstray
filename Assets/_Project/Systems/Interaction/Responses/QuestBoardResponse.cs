using UnityEngine;

public class QuestBoardResponse : MonoBehaviour, IInteractionResponse
{
    [Tooltip("Optional override. Leave empty to use the persistent gameplay UI controller.")]
    [SerializeField] private QuestBoardController questBoard;

    [Header("One-time Intro")]
    [SerializeField]
    private string introDialogueID = "chapter1.questBoardIntro";

    [SerializeField]
    private string introProgressionFlag =
        GameProgressionFlags.Chapter1QuestBoardIntroComplete;

    public void OnInteract()
    {
        if (ShouldPlayIntroDialogue())
        {
            DialogueManager.Instance.StartDialogue(
                introDialogueID,
                () =>
                {
                    GameProgressionManager.Instance?.SetFlag(
                        introProgressionFlag);

                    OpenBoard();
                });

            return;
        }

        OpenBoard();
    }

    private bool ShouldPlayIntroDialogue()
    {
        if (string.IsNullOrWhiteSpace(introDialogueID))
            return false;

        if (DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
        {
            return false;
        }

        if (GameProgressionManager.Instance == null)
            return true;

        return !GameProgressionManager.Instance.HasFlag(
            introProgressionFlag);
    }

    private void OpenBoard()
    {
        if (questBoard == null)
            questBoard = GameplayUIManager.Instance?.QuestBoard;

        if (questBoard == null)
        {
            Debug.LogError(
                "QuestBoardResponse could not find the persistent QuestBoardController.");
            return;
        }

        questBoard.OpenBoard();
    }
}
