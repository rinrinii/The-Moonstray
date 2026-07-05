using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Death")]
    [SerializeField] private float gameOverDelay = 0.4f;

    public event Action OnStoryDeath;

    private Animator currentAnimator;

    private PlayerTransformation transformation;
    private PlayerMovement movement;
    private PlayerClimbing climbing;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public bool IsDead => isDead;

    private bool isDead;
    private bool suppressNextGameOver;

    private HUDController hudController;

    private void Start()
    {
        currentHealth = maxHealth;

        hudController = FindFirstObjectByType<HUDController>();

        transformation = GetComponent<PlayerTransformation>();
        movement = GetComponent<PlayerMovement>();
        climbing = GetComponent<PlayerClimbing>();

        UpdateAnimatorReference();
    }

    // =========================================
    // DEBUG
    // =========================================

    // Instantly restore the player's health to
    // maximum. This shortcut is intentionally
    // available in development builds for
    // quickly testing combat encounters.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            RestoreFullHealth();
        }
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

        Debug.Log($"Player Health: {currentHealth}");

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

    public void ReviveAtFullHealth()
    {
        currentHealth = maxHealth;

        isDead = false;

        UpdateAnimatorReference();

        if (movement != null)
            movement.enabled = true;

        if (climbing != null)
            climbing.enabled = true;

        if (currentAnimator != null)
            currentAnimator.Rebind();

        Debug.Log("Player revived.");
    }

    public void SuppressNextGameOver()
    {
        suppressNextGameOver = true;
    }

    private void Die()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        UpdateAnimatorReference();

        if (currentAnimator != null)
            currentAnimator.SetTrigger("Die");

        if (movement != null)
            movement.enabled = false;

        if (climbing != null)
            climbing.enabled = false;

        yield return new WaitForSeconds(gameOverDelay);

        if (suppressNextGameOver)
        {
            suppressNextGameOver = false;

            OnStoryDeath?.Invoke();

            yield break;
        }

        hudController?.ShowGameOver();
    }

    private void UpdateAnimatorReference()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            if (animator.gameObject.activeInHierarchy)
            {
                currentAnimator = animator;
                return;
            }
        }

        currentAnimator = null;
    }

    public void RestoreAfterStoryDeath(float healthPercent = 0.10f)
    {
        currentHealth = maxHealth * Mathf.Clamp01(healthPercent);

        isDead = false;
        suppressNextGameOver = false;

        UpdateAnimatorReference();

        if (movement != null)
            movement.enabled = true;

        if (climbing != null)
            climbing.enabled = true;

        if (currentAnimator != null)
        {
            currentAnimator.Rebind();
            currentAnimator.Update(0f);

            // Return to a neutral standing state.
            currentAnimator.Play("Idle", 0, 0f);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;

        isDead = false;
        suppressNextGameOver = false;

        UpdateAnimatorReference();

        if (movement != null)
            movement.enabled = true;

        if (climbing != null)
            climbing.enabled = true;

        if (currentAnimator != null)
        {
            currentAnimator.Rebind();
            currentAnimator.Update(0f);
            currentAnimator.Play("Idle", 0, 0f);
        }

        Debug.Log("Player health restored.");
    }
}