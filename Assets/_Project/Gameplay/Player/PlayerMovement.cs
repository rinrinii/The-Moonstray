using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float turnSmoothTime = 0.03f;
    public float sprintMultiplier = 1.8f;

    private Vector3 currentMoveDirection;

    private PlayerHowl howl;

    private CharacterController controller;
    private PlayerTransformation transformation;

    private PlayerClimbing climbing;
    private PlayerStamina stamina;

    private bool hasDoubleJumped;   // for double jump
    [SerializeField]
    private float doubleJumpMultiplier = 0.9f;

    private float groundedRememberTime = 0.1f;
    private float groundedRemember;

    private PlayerLunarSense lunarSense;

    private PlayerDash dash;

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

        dash =
            GetComponent<PlayerDash>();

        RefreshCameraReference();

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

    void Update()
    {
        // =========================================
        // CONTROLLER DISABLED
        // =========================================
        if (!controller.enabled)
            return;

        RefreshCameraReference();

        if (cam == null)
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
            if (dash != null)
            {
                dash.TryDash(direction);
            }
        }

        // =========================================
        // DASH MOVEMENT
        // =========================================
        if (dash != null && dash.HandleDash())
        {
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
            groundedRemember =
                groundedRememberTime;

            hasDoubleJumped = false;
        }
        else
        {
            groundedRemember -=
                Time.deltaTime;
        }

        // =========================================
        // JUMP
        // =========================================
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool normalJump =
                groundedRemember > 0;

            bool doubleJump =
                !normalJump &&
                !hasDoubleJumped &&
                UpgradeManager.Instance != null &&
                UpgradeManager.Instance.IsUnlocked(
                    UpgradeType.DoubleJump
                );

            if (normalJump || doubleJump)
            {
                float jumpHeight = transformation.GetJumpHeight();

                if (doubleJump)
                {
                    jumpHeight *=
                        doubleJumpMultiplier;
                }

                velocity.y =
                    Mathf.Sqrt(
                        jumpHeight *
                        -2f *
                        transformation.GetGravity()
                    );

                if (doubleJump)
                {
                    hasDoubleJumped = true;
                }

                if (currentAnim != null)
                {
                    if (doubleJump)
                    {
                        currentAnim.SetTrigger(
                            "DoubleJump"
                        );
                    }
                    else
                    {
                        currentAnim.SetTrigger(
                            "Jump"
                        );
                    }
                }
            }
        }

        // =========================================
        // SENSE
        // =========================================
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

            // Debug.Log("Animator Speed = " + currentAnim.GetFloat("Speed"));

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

    public bool CanUseCoyoteTime()
    {
        return groundedRemember > 0;
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

    private void RefreshCameraReference()
    {
        if (cam != null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cam = mainCamera.transform;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cam = null;
        RefreshCameraReference();
    }
}