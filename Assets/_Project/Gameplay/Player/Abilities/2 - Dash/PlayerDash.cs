using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField]
    private float dashSpeed = 20f;

    [SerializeField]
    private float dashDuration = 0.3f;

    [SerializeField]
    private float dashCooldown = 1.5f;

    private bool isDashing;

    private float dashTime;

    private float nextDashTime;

    private Vector3 dashDirection;

    private CharacterController controller;

    private PlayerTransformation transformation;

    private PlayerStamina stamina;

    private Animator currentAnim;

    private Transform cam;

    private PlayerMovement movement;

    private void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        transformation =
            GetComponent<PlayerTransformation>();

        stamina =
            GetComponent<PlayerStamina>();

        movement =
            GetComponent<PlayerMovement>();

        cam =
            Camera.main.transform;
    }

    private Animator GetCurrentAnimator()
    {
        return GetComponentInChildren<Animator>();
    }

    public void TryDash(Vector3 direction)
    {
        if (!AbilityManager.Instance.IsUnlocked(
        AbilityType.Dash))
        {
            return;
        }

        if (Time.time < nextDashTime)
            return;

        if (isDashing)
            return;

        if (transformation.currentForm !=
            PlayerTransformation.FormState.Wolf)
        {
            return;
        }

        if (!movement.CanUseCoyoteTime())
            return;

        if (!stamina.CanDash())
            return;

        currentAnim =
            GetCurrentAnimator();

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

        dashDirection =
            transform.forward;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(
                    direction.x,
                    direction.z
                )
                * Mathf.Rad2Deg +
                cam.eulerAngles.y;

            dashDirection =
                Quaternion.Euler(
                    0f,
                    targetAngle,
                    0f
                ) *
                Vector3.forward;
        }

        stamina.UseDashStamina();

        isDashing = true;

        dashTime =
            dashDuration;

        nextDashTime =
            Time.time +
            dashCooldown;

        currentAnim.SetBool(
            "IsDashing",
            true
        );

        currentAnim.SetTrigger(
            "Dash"
        );
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool HandleDash()
    {
        if (!isDashing)
            return false;

        controller.Move(
            dashDirection.normalized *
            dashSpeed *
            Time.deltaTime
        );

        dashTime -= Time.deltaTime;

        if (dashTime <= 0)
        {
            isDashing = false;

            currentAnim =
                GetCurrentAnimator();

            if (currentAnim != null)
            {
                currentAnim.SetBool(
                    "IsDashing",
                    false
                );
            }
        }

        return true;
    }
}