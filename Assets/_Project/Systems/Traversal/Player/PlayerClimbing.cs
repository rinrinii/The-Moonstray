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
    private Animator currentAnim;

    private ClimbableObject currentClimbable;

    private bool isClimbing;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();

        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        currentAnim = GetComponentInChildren<Animator>();
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

        float vertical = Input.GetAxisRaw("Vertical");

        if (!isClimbing &&
            currentClimbable != null &&
            vertical > 0.1f)
        {
            StartClimbing();
        }

        // =========================================
        // CLIMB MOVEMENT
        // =========================================

        if (isClimbing)
        {
            HandleClimbing();
        }
    }

    void DetectClimbable()
    {
        Collider[] hits = Physics.OverlapSphere(
            climbCheck.position,
            climbCheckRadius,
            climbLayer
        );

        currentClimbable = null;

        foreach (Collider hit in hits)
        {
            Debug.Log("Detected: " + hit.name);

            ClimbableObject climbable =
                hit.GetComponent<ClimbableObject>();

            if (climbable != null)
            {
                Debug.Log("Climbable found!");

                currentClimbable = climbable;
                break;
            }
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
    }

    void StopClimbing()
    {
        isClimbing = false;

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