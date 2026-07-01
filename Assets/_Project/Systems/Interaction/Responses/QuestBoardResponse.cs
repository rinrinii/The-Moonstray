using UnityEngine;

public class QuestBoardResponse : MonoBehaviour, IInteractionResponse
{
    [SerializeField] private QuestBoardController questBoard;

    public void OnInteract()
    {
        if (questBoard == null)
        {
            Debug.LogError("QuestBoardController is missing.");
            return;
        }

        questBoard.OpenBoard();
    }
}