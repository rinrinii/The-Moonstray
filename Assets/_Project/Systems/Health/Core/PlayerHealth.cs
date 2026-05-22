using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Death")]
    [SerializeField] private float gameOverDelay = 0.4f;

    private Animator currentAnimator;

    private PlayerTransformation transformation;
    private PlayerMovement movement;
    private PlayerClimbing climbing;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private bool isDead;

    // Temporary direct reference
    // Refactor later into events/UI manager
    private HUDController hudController;

    private void Start()
    {
        currentHealth = maxHealth;

        hudController =
            FindFirstObjectByType<HUDController>();

        transformation =
            GetComponent<PlayerTransformation>();

        movement =
            GetComponent<PlayerMovement>();

        climbing =
            GetComponent<PlayerClimbing>();

        UpdateAnimatorReference();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        Debug.Log(
            $"Player Health: {currentHealth}"
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );
    }

    private void Die()
    {
        StartCoroutine(
            DeathSequence()
        );
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        Debug.Log("Player Died");

        // Update current active animator
        UpdateAnimatorReference();

        // =========================================
        // PLAY DEATH ANIMATION
        // =========================================

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger(
                "Die"
            );
        }

        // =========================================
        // DISABLE PLAYER CONTROL
        // =========================================

        if (movement != null)
        {
            movement.enabled = false;
        }

        if (climbing != null)
        {
            climbing.enabled = false;
        }

        // =========================================
        // WAIT BEFORE GAME OVER
        // =========================================

        yield return new WaitForSeconds(
            gameOverDelay
        );

        // =========================================
        // SHOW GAME OVER UI
        // =========================================

        if (hudController != null)
        {
            hudController.ShowGameOver();
        }
    }

    private void UpdateAnimatorReference()
    {
        currentAnimator =
            GetComponentInChildren<Animator>();
    }
}