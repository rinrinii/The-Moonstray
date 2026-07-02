using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementTutorialTracker : MonoBehaviour
{
    private CharacterController controller;

    private bool moveCompleted;
    private bool sprintCompleted;
    private bool jumpCompleted;

    private float jumpInputGraceTimer;
    private const float JumpInputGraceDuration = 0.15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged += HandleTutorialStepChanged;
    }

    private void OnDisable()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged -= HandleTutorialStepChanged;
    }

    private void HandleTutorialStepChanged(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.Move:
                moveCompleted = false;
                break;

            case TutorialStep.Sprint:
                sprintCompleted = false;
                break;

            case TutorialStep.Jump:
                jumpCompleted = false;
                jumpInputGraceTimer = 0f;
                break;
        }
    }

    private void Update()
    {
        if (TutorialManager.Instance == null)
            return;

        if (TutorialManager.Instance.IsTutorialFinished)
            return;

        switch (TutorialManager.Instance.CurrentStep)
        {
            case TutorialStep.Move:
                CheckMovement();
                break;

            case TutorialStep.Sprint:
                CheckSprint();
                break;

            case TutorialStep.Jump:
                CheckJump();
                break;
        }
    }

    private void CheckMovement()
    {
        if (moveCompleted)
            return;

        Vector2 movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        if (movementInput.sqrMagnitude > 0f)
        {
            moveCompleted = true;

            TutorialManager.Instance.CompleteCurrentStep();
        }
    }

    private void CheckSprint()
    {
        if (sprintCompleted)
            return;

        Vector2 movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        bool moving = movementInput.sqrMagnitude > 0f;

        if (moving && Input.GetKey(KeyCode.LeftShift))
        {
            sprintCompleted = true;

            TutorialManager.Instance.CompleteCurrentStep();
        }
    }

    private void CheckJump()
    {
        if (jumpCompleted)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpInputGraceTimer = JumpInputGraceDuration;

        if (jumpInputGraceTimer > 0f)
            jumpInputGraceTimer -= Time.deltaTime;

        if (jumpInputGraceTimer > 0f)
        {
            jumpCompleted = true;

            TutorialManager.Instance.CompleteCurrentStep();
        }
    }
}