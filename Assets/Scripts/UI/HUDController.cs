using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite humanPortrait;
    public Sprite wolfPortrait;

    [Header("Health Bar Colors")]
    [SerializeField] private Color healthyColor = new Color32(0x45, 0x89, 0x45, 0xFF); // #458945
    [SerializeField] private Color warningColor = new Color32(0xEA, 0xDD, 0x45, 0xFF); // #EADD45
    [SerializeField] private Color dangerColor = new Color32(0xD7, 0x1C, 0x1C, 0xFF); // #D71C1C

    private VisualElement hpOverlay;

    private VisualElement playerFormIcon;
    private VisualElement staminaFill;
    private VisualElement healthFill;

    private PlayerTransformation playerTransformation;
    private PlayerStamina playerStamina;
    private PlayerHealth playerHealth;

    // Allocated reference representations to bypass runtime engine garbage collection
    private StyleBackground humanStyleBg;
    private StyleBackground wolfStyleBg;
    private PlayerTransformation.FormState lastFormState;

    private void Start()
    {
        var ui = GameplayUIManager.Instance;
        VisualElement root = ui.RootVisualElement;

        playerFormIcon = root.Q<VisualElement>("PlayerFormIcon");
        staminaFill = root.Q<VisualElement>("StaminaFill");
        healthFill = root.Q<VisualElement>("HealthFill");
        hpOverlay = root.Q<VisualElement>("HPOverlay");

        // Cache backgrounds to avoid GC allocation spikes in Update
        if (humanPortrait != null) humanStyleBg = new StyleBackground(humanPortrait);
        if (wolfPortrait != null) wolfStyleBg = new StyleBackground(wolfPortrait);

        playerTransformation = FindFirstObjectByType<PlayerTransformation>();
        playerStamina = FindFirstObjectByType<PlayerStamina>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerTransformation != null) lastFormState = playerTransformation.currentForm;

        InitialForceRefresh();
    }

    private void Update()
    {
        UpdatePortrait();
        UpdateStaminaBar();
        UpdateHealthBar();
    }

    private void InitialForceRefresh()
    {
        if (playerTransformation != null && playerFormIcon != null)
        {
            playerFormIcon.style.backgroundImage = (playerTransformation.currentForm == PlayerTransformation.FormState.Human) ? humanStyleBg : wolfStyleBg;
        }
        UpdateStaminaBar();
        UpdateHealthBar();
    }

    private void UpdatePortrait()
    {
        if (playerTransformation == null || playerFormIcon == null) return;

        if (playerTransformation.currentForm != lastFormState)
        {
            lastFormState = playerTransformation.currentForm;
            playerFormIcon.style.backgroundImage = (lastFormState == PlayerTransformation.FormState.Human) ? humanStyleBg : wolfStyleBg;
        }
    }

    private void UpdateStaminaBar()
    {
        if (playerStamina == null || staminaFill == null) return;
        staminaFill.style.width = Length.Percent(playerStamina.GetStaminaPercent() * 100f);
    }

    private void UpdateHealthBar()
    {
        if (playerHealth == null || healthFill == null)
            return;

        float healthPercent =
            playerHealth.CurrentHealth / playerHealth.MaxHealth;

        // =========================================
        // HEALTH BAR WIDTH
        // =========================================

        healthFill.style.width =
            Length.Percent(healthPercent * 100f);

        // =========================================
        // HEALTH BAR COLOR
        // =========================================

        Color barColor;

        if (healthPercent > 0.60f)
        {
            float t = Mathf.InverseLerp(
                0.60f,
                1f,
                healthPercent
            );

            barColor = Color.Lerp(
                warningColor,
                healthyColor,
                t
            );
        }
        else
        {
            float t = Mathf.InverseLerp(
                0.30f,
                0.60f,
                healthPercent
            );

            barColor = Color.Lerp(
                dangerColor,
                warningColor,
                t
            );
        }

        healthFill.style.backgroundColor = barColor;

        // =========================================
        // LOW HP SCREEN OVERLAY
        // =========================================

        if (hpOverlay == null)
            return;

        if (healthPercent > 0.30f)
        {
            hpOverlay.style.opacity = 0f;
            return;
        }

        // Fade in between 30% and 0%
        float alpha = Mathf.Lerp(
            0.35f,                                     // Minimum opacity once HP is low
            1f,                                        // Maximum opacity at 0 HP
            Mathf.InverseLerp(
                0.30f,
                0f,
                healthPercent
            )
        );

        // Pulse below 15% HP
        if (healthPercent <= 0.15f)
        {
            float pulse =
                0.75f +
                0.25f *
                Mathf.Sin(Time.time * 4f);

            alpha *= pulse;
        }

        hpOverlay.style.opacity = alpha;
    }

    // =========================================
    // GAME OVER
    // =========================================

    /// <summary>
    /// Displays the game over overlay panel. Called by PlayerHealth.cs when health reaches 0.
    /// </summary>
    public void ShowGameOver()
    {
        // Target the master manager's container reference safely
        if (GameplayUIManager.Instance != null && GameplayUIManager.Instance.GameOverContainer != null)
        {
            GameplayUIManager.Instance.GameOverContainer.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f; // Freeze game mechanics on player death
        }
    }
}