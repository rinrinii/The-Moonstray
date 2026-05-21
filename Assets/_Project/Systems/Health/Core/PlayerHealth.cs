using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private bool isDead;
    private HUDController hudController;    // modify later since health shouldn't know abt UI

    private void Start()
    {
        currentHealth = maxHealth;
        hudController = FindFirstObjectByType<HUDController>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player Health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Player Died");
        Time.timeScale = 0f;

        if (hudController != null)
        {
            hudController.ShowGameOver();
        }
    }
}