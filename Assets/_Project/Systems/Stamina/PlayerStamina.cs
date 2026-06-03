using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 100f;

    [Header("Regeneration")]
    public float regenRate = 25f;

    [Header("Sprint")]
    public float sprintDrainRate = 12f;
    public float minimumSprintStamina = 5f;

    [Header("Dash")]
    public float dashCost = 25f;

    [Header("Climb")]
    public float climbMoveDrainRate = 18f;
    public float climbIdleDrainRate = 6f;
    public float minimumClimbStamina = 15f;

    private float currentStamina;

    private bool canRegenerate = true;

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        Regenerate();
    }

    void Regenerate()
    {
        if (!canRegenerate)
            return;

        if (currentStamina < maxStamina)
        {
            currentStamina +=
                regenRate *
                Time.deltaTime;

            currentStamina =
                Mathf.Clamp(
                    currentStamina,
                    0,
                    maxStamina
                );
        }
    }

    // =========================================
    // STAMINA CHECKS
    // =========================================

    public bool HasStamina()
    {
        return currentStamina > 0;
    }

    public bool CanClimb()
    {
        return currentStamina >=
            minimumClimbStamina;
    }

    public bool CanSprint()
    {
        return currentStamina >=
            minimumSprintStamina;
    }

    public bool CanDash()
    {
        return currentStamina >= dashCost;
    }

    // =========================================
    // STAMINA USAGE
    // =========================================

    public void UseSprintStamina()
    {
        UseStamina(
            sprintDrainRate *
            Time.deltaTime
        );
    }

    public void UseDashStamina()
    {
        UseStamina(dashCost);
    }

    public void UseClimbMoveStamina(
        float multiplier = 1f)
    {
        UseStamina(
            climbMoveDrainRate *
            multiplier *
            Time.deltaTime
        );
    }

    public void UseClimbIdleStamina(
        float multiplier = 1f)
    {
        UseStamina(
            climbIdleDrainRate *
            multiplier *
            Time.deltaTime
        );
    }

    void UseStamina(float amount)
    {
        currentStamina -= amount;

        currentStamina =
            Mathf.Clamp(
                currentStamina,
                0,
                maxStamina
            );

        Debug.Log(
            "Stamina Used: " +
            amount.ToString("F3") +
            " | Remaining: " +
            currentStamina.ToString("F2") +
            " / " +
            maxStamina
        );
    }

    // =========================================
    // REGEN CONTROL
    // =========================================

    public void BlockRegeneration()
    {
        canRegenerate = false;
    }

    public void AllowRegeneration()
    {
        canRegenerate = true;
    }

    // =========================================
    // GETTERS
    // =========================================

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetMaxStamina()
    {
        return maxStamina;
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}