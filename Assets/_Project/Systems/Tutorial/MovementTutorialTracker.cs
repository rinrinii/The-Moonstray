using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementTutorialTracker : MonoBehaviour
{
    private CharacterController controller;

    private bool moveCompleted;
    private bool sprintCompleted;
    private bool jumpCompleted;

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
                break;
        }
    }

    private void Update()
    {
        if (TutorialManager.Instance == null)
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

            TutorialManager.Instance.SetStep(
                TutorialStep.Sprint);
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

            TutorialManager.Instance.SetStep(
                TutorialStep.Jump);
        }
    }

    private void CheckJump()
    {
        if (jumpCompleted)
            return;

        if (Input.GetKeyDown(KeyCode.Space) &&
            controller.isGrounded)
        {
            jumpCompleted = true;

            TutorialManager.Instance.SetStep(
                TutorialStep.ReachCourtyard);
        }
    }
}