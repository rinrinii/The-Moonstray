using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float turnSmoothTime = 0.1f;
    public float gravity = -9.81f;
    public float sprintMultiplier = 1.5f;

    private CharacterController controller;
    private PlayerTransformation transformation;
    private Animator currentAnim;
    private Transform cam;
    private Vector3 velocity;
    private float turnSmoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        transformation = GetComponent<PlayerTransformation>();
        cam = Camera.main.transform;
        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        currentAnim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Disable movement during pause
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
            }

            return;
        }

        // Disable movement during dialogue
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
            }

            ApplyGravity();
            return;
        }

        // Disable movement during transformation states
        if (!transformation.CanMove())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
            }

            ApplyGravity();
            return;
        }

        float baseSpeed = transformation.GetSpeed();
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? baseSpeed * sprintMultiplier : baseSpeed;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(x, 0f, z).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg +
                cam.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir =
                Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }

        if (currentAnim != null)
        {
            float speedPercent = direction.magnitude * currentSpeed;

            currentAnim.SetFloat(
                "Speed",
                speedPercent,
                0.15f,
                Time.deltaTime
            );
        }

        ApplyGravity();
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}