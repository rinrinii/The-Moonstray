using System.Collections;
using UnityEngine;

public class PlayerClimbing : MonoBehaviour
{
    [Header("References")]
    public Transform climbCheck;

    [Header("Detection")]
    public float climbCheckRadius = 0.2f;
    public LayerMask climbLayer;

    [Header("Exit Settings")]
    public float climbReEnterDelay = 0.3f;

    [SerializeField]
    private float climbFinishUpBoost = 1.1f;

    [SerializeField]
    private float climbFinishForwardBoost = 0.5f;

    [Header("Root Anchor")]
    [SerializeField]
    private float rootAnchorMoveMultiplier = 0.5f;

    [SerializeField]
    private float rootAnchorIdleMultiplier = 0.5f;

    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerStamina stamina;
    private PlayerTransformation transformation;

    private Animator currentAnim;

    private ClimbableObject currentClimbable;
    private ClimbableObject lastDetectedClimbable;

    private bool isClimbing;
    private bool canStartClimb = true;

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

        if (!AbilityManager.Instance.IsUnlocked(AbilityType.Climb))
        {
            return;
        }

        float vertical =
            Input.GetAxisRaw("Vertical");

        // =========================================
        // ENTER CLIMB
        // =========================================

        if (!isClimbing &&
            canStartClimb &&
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

            bool hasRootAnchor =
                UpgradeManager.Instance != null &&
                UpgradeManager.Instance.IsUnlocked(
                    UpgradeType.RootAnchor
                );

            if (Mathf.Abs(vertical) > 0.1f)
            {
                if (hasRootAnchor)
                {
                    stamina.UseClimbMoveStamina(
                        rootAnchorMoveMultiplier
                    );
                }
                else
                {
                    stamina.UseClimbMoveStamina();
                }
            }
            else
            {
                if (hasRootAnchor)
                {
                    stamina.UseClimbIdleStamina(
                        rootAnchorIdleMultiplier
                    );
                }
                else
                {
                    stamina.UseClimbIdleStamina();
                }
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

        if (movement != null)
        {
            movement.ResetVerticalVelocity();
        }

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

    public void StopClimbing()
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

    IEnumerator EnableClimbAgain()
    {
        canStartClimb = false;

        yield return new WaitForSeconds(
            climbReEnterDelay
        );

        canStartClimb = true;
    }

    void HandleClimbing()
    {
        float vertical =
            Input.GetAxisRaw("Vertical");

        // =========================================
        // REACHED TOP
        // =========================================

        if (currentClimbable == null &&
            vertical > 0.1f)
        {
            FinishClimb();
            return;
        }

        // =========================================
        // FELL AWAY FROM WALL
        // =========================================

        if (currentClimbable == null)
        {
            StopClimbing();
            return;
        }

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
        // MANUAL EXIT
        // =========================================

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopClimbing();
        }
    }

    void FinishClimb()
    {
        Debug.Log("Finished Climb");

        StopClimbing();

        Vector3 finishPosition =
            transform.position +
            Vector3.up * climbFinishUpBoost +
            transform.forward * climbFinishForwardBoost;

        StartCoroutine(
            SmoothClimbFinish(finishPosition)
        );

        StartCoroutine(
            EnableClimbAgain()
        );
    }

    IEnumerator SmoothClimbFinish(
    Vector3 targetPosition)
    {
        controller.enabled = false;

        Vector3 startPosition =
            transform.position;

        float duration = 0.18f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed / duration;

            // Smooth easing
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        transform.position =
            targetPosition;

        controller.enabled = true;
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