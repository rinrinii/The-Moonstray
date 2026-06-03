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

    [Header("Cleanse Dash")]
    [SerializeField]
    private float cleanseRadius = 1f;

    [Header("Chain Dash")]
    private int currentDashCharges;
    private int maxDashCharges;

    private bool isDashing;
    private float dashTime;
    private float nextDashTime;

    private Vector3 dashDirection;

    private CharacterController controller;
    private PlayerTransformation transformation;
    private PlayerStamina stamina;
    private PlayerMovement movement;

    private Animator currentAnim;
    private Transform cam;

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

        UpdateDashCharges();

        currentDashCharges =
            maxDashCharges;
    }

    private Animator GetCurrentAnimator()
    {
        return GetComponentInChildren<Animator>();
    }

    public void TryDash(
        Vector3 direction)
    {
        if (!AbilityManager.Instance.IsUnlocked(
            AbilityType.Dash))
        {
            return;
        }

        if (currentDashCharges <= 0)
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

        transformation.SetIgnoreSpeedModifiers(
            true
        );

        dashTime =
            dashDuration;

        currentDashCharges--;

        Debug.Log(
            $"Dash Charges: " +
            currentDashCharges +
            "/" +
            maxDashCharges
        );

        if (currentDashCharges <= 0)
        {
            nextDashTime =
                Time.time +
                dashCooldown;
        }

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
        UpdateDashCharges();

        if (currentDashCharges <= 0 &&
            Time.time >= nextDashTime)
        {
            currentDashCharges =
                maxDashCharges;

            Debug.Log(
                $"Dash Refilled: " +
                currentDashCharges +
                "/" +
                maxDashCharges
            );
        }

        if (!isDashing)
            return false;

        // =========================================
        // CLEANSE DASH
        // =========================================

        if (UpgradeManager.Instance != null &&
            UpgradeManager.Instance.IsUnlocked(
                UpgradeType.CleanseDash
            ))
        {
            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    cleanseRadius
                );

            foreach (Collider hit in hits)
            {
                ExpandingWaning waning =
                    hit.GetComponent<
                        ExpandingWaning>();

                if (waning != null)
                {
                    waning.Cleanse();
                }
            }
        }

        // =========================================
        // DASH MOVEMENT
        // =========================================

        controller.Move(
            dashDirection.normalized *
            dashSpeed *
            Time.deltaTime
        );

        dashTime -= Time.deltaTime;

        if (dashTime <= 0)
        {
            isDashing = false;

            transformation.SetIgnoreSpeedModifiers(
                false
            );

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

    private void UpdateDashCharges()
    {
        maxDashCharges =
            UpgradeManager.Instance != null &&
            UpgradeManager.Instance.IsUnlocked(
                UpgradeType.ChainDash
            )
            ? 2
            : 1;
    }
}