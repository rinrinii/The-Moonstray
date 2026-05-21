using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    [Header("References")]
    public Transform climbCheck;

    [Header("Detection")]
    public float climbCheckRadius = 0.5f;
    public LayerMask climbLayer;

    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerStamina stamina;

    private Animator currentAnim;

    private ClimbableObject currentClimbable;
    private ClimbableObject lastDetectedClimbable;
    private PlayerTransformation transformation;

    private bool isClimbing;


    void Start()
    {
        controller =
            GetComponent<CharacterController>();

        movement =
            GetComponent<PlayerMovement>();

        stamina =
            GetComponent<PlayerStamina>();

        transformation =
            GetComponent<PlayerTransformation>();

        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        currentAnim =
            GetComponentInChildren<Animator>();
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }

    void Update()
    {
        DetectClimbable();

        // =========================================
        // ENTER CLIMB
        // =========================================

        float vertical =
            Input.GetAxisRaw("Vertical");

        if (!isClimbing &&
            currentClimbable != null &&
            vertical > 0.1f &&
            stamina.CanClimb() &&
            transformation.currentForm ==
            PlayerTransformation.FormState.Human)
        {
            StartClimbing();
        }

        // =========================================
        // CLIMB MOVEMENT
        // =========================================

        if (isClimbing)
        {
            stamina.BlockRegeneration();
            // Stop climb if no stamina
            if (!stamina.HasStamina())
            {
                StopClimbing();
                return;
            }

            if (Mathf.Abs(vertical) > 0.1f)
            {
                stamina.UseClimbMoveStamina();
            }
            else
            {
                stamina.UseClimbIdleStamina();
            }

            HandleClimbing();
        }
    }

    void DetectClimbable()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                climbCheck.position,
                climbCheckRadius,
                climbLayer
            );

        currentClimbable = null;

        foreach (Collider hit in hits)
        {
            ClimbableObject climbable =
                hit.GetComponent<ClimbableObject>();

            if (climbable != null)
            {
                currentClimbable = climbable;
                break;
            }
        }

        // =========================================
        // DEBUG LOGGING
        // =========================================

        if (currentClimbable != lastDetectedClimbable)
        {
            if (currentClimbable != null)
            {
                Debug.Log(
                    $"Climbable detected: {currentClimbable.name}"
                );
            }
            else
            {
                Debug.Log(
                    "No climbable detected"
                );
            }

            lastDetectedClimbable =
                currentClimbable;
        }
    }

    void StartClimbing()
    {
        isClimbing = true;

        if (currentAnim != null)
        {
            currentAnim.SetBool(
                "IsClimbing",
                true
            );

            currentAnim.SetFloat(
                "ClimbSpeed",
                0f
            );
        }

        Debug.Log("Started Climbing");
    }

    void StopClimbing()
    {
        isClimbing = false;
        stamina.AllowRegeneration();
        if (currentAnim != null)
        {
            currentAnim.SetBool(
                "IsClimbing",
                false
            );

            currentAnim.SetFloat(
                "ClimbSpeed",
                0f
            );
        }

        Debug.Log("Stopped Climbing");
    }

    void HandleClimbing()
    {
        if (currentClimbable == null)
        {
            StopClimbing();
            return;
        }

        float vertical =
            Input.GetAxisRaw("Vertical");

        // =========================================
        // CLIMB MOVEMENT
        // =========================================

        Vector3 climbMove =
            Vector3.up *
            vertical *
            currentClimbable.climbSpeed;

        controller.Move(
            climbMove * Time.deltaTime
        );

        // =========================================
        // ANIMATOR
        // =========================================

        if (currentAnim != null)
        {
            currentAnim.SetFloat(
                "ClimbSpeed",
                Mathf.Abs(vertical)
            );
        }

        // =========================================
        // EXIT CLIMB
        // =========================================

        // Jump off wall
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopClimbing();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (climbCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            climbCheck.position,
            climbCheckRadius
        );
    }
}