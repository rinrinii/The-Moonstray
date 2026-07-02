using UnityEngine;

public class TutorialDirector : MonoBehaviour
{
    private GameplayUIManager ui;

    private void Awake()
    {
        ui = GameplayUIManager.Instance;
    }

    private void OnEnable()
    {
        if (TutorialManager.Instance == null)
            return;

        TutorialManager.Instance.OnStepChanged += HandleStepChanged;

        // Synchronize immediately with the current tutorial step.
        HandleStepChanged(TutorialManager.Instance.CurrentStep);
    }

    private void OnDisable()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged -= HandleStepChanged;
    }

    private void HandleStepChanged(TutorialStep step)
    {
        if (ui == null)
            return;

        switch (step)
        {
            // Pinewatch Trail
            case TutorialStep.Move:

                ui.Objectives.Show();

                ui.Prompt.Show(
                    "[ WASD ]  Movement",
                    "Use WASD to move around."
                );

                break;

            case TutorialStep.Sprint:

                ui.Prompt.Show(
                    "[ LEFT SHIFT ]  Sprint",
                    "Hold Left Shift while moving."
                );

                break;

            case TutorialStep.Jump:

                ui.Prompt.Show(
                    "[ SPACE ]  Jump",
                    "Press Space to jump."
                );

                break;

            case TutorialStep.ReachCourtyard:

                ui.Prompt.Show(
                    "Reach the Courtyard",
                    "Follow the snowy trail ahead."
                );

                break;

            case TutorialStep.Finished:

                ui.Prompt.Hide();
                ui.Objectives.Hide();

                break;
        }
    }
}