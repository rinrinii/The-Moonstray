using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float turnSmoothTime = 0.03f;
    public float sprintMultiplier = 1.2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.5f;

    private bool isDashing;
    private float dashTime;
    private float nextDashTime;
    private Vector3 dashDirection;
    private Vector3 currentMoveDirection;

    private PlayerHowl howl;

    private CharacterController controller;
    private PlayerTransformation transformation;

    private PlayerClimbing climbing;
    private PlayerStamina stamina;

    private float groundedRememberTime = 0.1f;
    private float groundedRemember;

    private PlayerLunarSense lunarSense;

    private Animator currentAnim;
    private Transform cam;

    private Vector3 velocity;
    private float turnSmoothVelocity;

    

    void Start()
    {
        controller = GetComponent<CharacterController>();

        transformation =
            GetComponent<PlayerTransformation>();

        climbing =
            GetComponent<PlayerClimbing>();

        stamina =
            GetComponent<PlayerStamina>();

        lunarSense =
            GetComponent<PlayerLunarSense>();

        howl =
            GetComponent<PlayerHowl>();

        cam = Camera.main.transform;

        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        currentAnim = GetComponentInChildren<Animator>();

        if (climbing != null)
        {
            climbing.UpdateAnimator();
        }
    }

    // =========================================
    // INTERACTION ANIMATIONS
    // =========================================
    public void PlayInteractionAnimation(
        InteractionType type)
    {
        if (currentAnim == null) return;

        if (type == InteractionType.None)
        {
            return;
        }

        if (type == InteractionType.Kneel)
        {
            currentAnim.SetTrigger("InteractKneel");
        }
        else
        {
            currentAnim.SetTrigger("InteractStand");
        }
    }

    // =========================================
    // DASH
    // =========================================
    void TryDash(Vector3 direction)
    {
        if (Time.time < nextDashTime)
            return;

        if (isDashing)
            return;

        if (transformation.currentForm !=
            PlayerTransformation.FormState.Wolf)
        {
            return;
        }

        if (groundedRemember <= 0)
            return;

        if (!stamina.CanDash())
            return;

        if (currentAnim == null)
            return;

        AnimatorStateInfo state =
            currentAnim.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Howl") ||
            state.IsName("Interact-Kneel") ||
            state.IsName("Interact-Stand"))
        {
            return;
        }

        dashDirection = transform.forward;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(direction.x, direction.z)
                * Mathf.Rad2Deg +
                cam.eulerAngles.y;

            dashDirection =
                Quaternion.Euler(0f, targetAngle, 0f)
                * Vector3.forward;
        }

        stamina.UseDashStamina();
        isDashing = true;
        dashTime = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        groundedRemember = 0;

        currentAnim.SetBool("IsDashing", true);
        currentAnim.SetTrigger("Dash");
    }

    void Update()
    {
        // =========================================
        // CONTROLLER DISABLED
        // =========================================
        if (!controller.enabled)
            return;


        // =========================================
        // CLIMBING
        // =========================================
        if (climbing != null && climbing.IsClimbing())
        {
            velocity.y = 0f;

            if (currentAnim != null)
            {
                currentAnim.SetFloat(
                    "Speed",
                    0,
                    0.15f,
                    Time.deltaTime
                );
            }

            return;
        }

        // =========================================
        // PAUSE
        // =========================================
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat(
                    "Speed",
                    0,
                    0.15f,
                    Time.deltaTime
                );
            }

            return;
        }

        // =========================================
        // DIALOGUE
        // =========================================
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat(
                    "Speed",
                    0,
                    0.15f,
                    Time.deltaTime
                );
            }

            ApplyGravity();
            return;
        }

        // =========================================
        // TRANSFORMATION LOCK
        // =========================================
        if (!transformation.CanMove())
        {
            if (currentAnim != null)
            {
                currentAnim.SetFloat(
                    "Speed",
                    0,
                    0.15f,
                    Time.deltaTime
                );
            }

            ApplyGravity();
            return;
        }

        float baseSpeed = transformation.GetSpeed();

        bool wantsToSprint =
            Input.GetKey(KeyCode.LeftShift);

        bool isSprinting =
            wantsToSprint &&
            stamina.CanSprint();

        float currentSpeed =
            isSprinting
            ? baseSpeed * sprintMultiplier
            : baseSpeed;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 direction =
            new Vector3(x, 0f, z).normalized;

        // =========================================
        // DASH INPUT
        // =========================================
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            TryDash(direction);
        }

        // =========================================
        // DASH MOVEMENT
        // =========================================
        if (isDashing)
        {
            controller.Move(
                dashDirection.normalized *
                dashSpeed *
                Time.deltaTime
            );

            dashTime -= Time.deltaTime;

            if (dashTime <= 0)
            {
                isDashing = false;

                if (currentAnim != null)
                {
                    currentAnim.SetBool(
                        "IsDashing",
                        false
                    );
                }
            }

            ApplyGravity();
            return;
        }

        // =========================================
        // MOVEMENT
        // =========================================
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(direction.x, direction.z)
                * Mathf.Rad2Deg +
                cam.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

            transform.rotation =
                Quaternion.Euler(0f, angle, 0f);

            currentMoveDirection =
                Quaternion.Euler(0f, angle, 0f)
                * Vector3.forward;

            controller.Move(
                currentMoveDirection *
                currentSpeed *
                Time.deltaTime
            );
        }

        // =========================================
        // COYOTE TIME
        // =========================================
        if (controller.isGrounded)
        {
            groundedRemember = groundedRememberTime;
        }
        else
        {
            groundedRemember -= Time.deltaTime;
        }

        // =========================================
        // JUMP
        // =========================================
        if (Input.GetKeyDown(KeyCode.Space) &&
            groundedRemember > 0)
        {
            velocity.y =
                Mathf.Sqrt(
                    transformation.GetJumpHeight() *
                    -2f *
                    transformation.GetGravity()
                );

            if (currentAnim != null)
            {
                currentAnim.SetTrigger("Jump");
            }
        }

        //test for sense
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (lunarSense != null)
            {
                lunarSense.ActivateSense();
            }
        }

        // =========================================
        // HOWL
        // =========================================
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (howl != null)
            {
                howl.ActivateHowl();
            }
        }

        // =========================================
        // STAMINA DRAIN
        // =========================================
        if (isSprinting && direction.magnitude >= 0.1f)
        {
            stamina.BlockRegeneration();

            stamina.UseSprintStamina();
        }
        else
        {
            stamina.AllowRegeneration();
        }

        // =========================================
        // ANIMATOR PARAMETERS
        // =========================================
        if (currentAnim != null)
        {
            float speedPercent =
                direction.magnitude * currentSpeed;

            currentAnim.SetFloat(
                "Speed",
                speedPercent,
                0.15f,
                Time.deltaTime
            );

            currentAnim.SetBool(
                "IsGrounded",
                controller.isGrounded
            );
        }

        ApplyGravity();
    }

    public void ResetVerticalVelocity()
    {
        velocity.y = 0f;
    }

    void ApplyGravity()
    {
        if (controller.isGrounded &&
            velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y +=
            transformation.GetGravity() *
            Time.deltaTime;

        controller.Move(
            velocity * Time.deltaTime
        );
    }
}