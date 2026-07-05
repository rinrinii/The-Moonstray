using UnityEngine;

public class QuestBoardResponse : MonoBehaviour, IInteractionResponse
{
    [Tooltip("Optional override. Leave empty to use the persistent gameplay UI controller.")]
    [SerializeField] private QuestBoardController questBoard;

    public void OnInteract()
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
