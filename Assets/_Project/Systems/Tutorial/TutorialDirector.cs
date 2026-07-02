using UnityEngine;

public class TutorialDirector : MonoBehaviour
{
    private void OnEnable()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged += HandleStepChanged;
    }

    private void OnDisable()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged -= HandleStepChanged;
    }

    private void HandleStepChanged(TutorialStep step)
    {
        switch (step)
        {
            //--------------------------------------------------
            // MOVE
            //--------------------------------------------------
            case TutorialStep.Move:

                GameplayUIManager.Instance.Prompt.Show(
                    "[ WASD ]  Movement",
                    "Use WASD to move around."
                );

                break;

            //--------------------------------------------------
            // SPRINT
            //--------------------------------------------------
            case TutorialStep.Sprint:

                GameplayUIManager.Instance.Prompt.Show(
                    "[ LEFT SHIFT ]  Sprint",
                    "Hold Left Shift while moving."
                );

                break;

            //--------------------------------------------------
            // JUMP
            //--------------------------------------------------
            case TutorialStep.Jump:

                GameplayUIManager.Instance.Prompt.Show(
                    "[ SPACE ]  Jump",
                    "Press Space to jump."
                );

                break;

            //--------------------------------------------------
            // REACH COURTYARD
            //--------------------------------------------------
            case TutorialStep.ReachCourtyard:

                GameplayUIManager.Instance.Prompt.Hide();

                break;

            //--------------------------------------------------
            // FINISHED
            //--------------------------------------------------
            case TutorialStep.Finished:

                GameplayUIManager.Instance.Prompt.Hide();

                QuestManager.Instance?.FinishQuest();

                break;
        }
    }
}