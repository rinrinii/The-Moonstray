using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite humanPortrait;
    public Sprite wolfPortrait;

    private UIDocument uiDocument;

    // =========================================
    // UI References
    // =========================================

    private VisualElement playerFormIcon;

    private VisualElement staminaFill;
    private VisualElement healthFill;

    private VisualElement gameOverContainer;

    // =========================================
    // Player References
    // =========================================

    private PlayerTransformation playerTransformation;
    private PlayerStamina playerStamina;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();

        VisualElement root =
            uiDocument.rootVisualElement;

        playerFormIcon =
            root.Q<VisualElement>(
                "PlayerFormIcon"
            );

        staminaFill =
            root.Q<VisualElement>(
                "StaminaFill"
            );

        healthFill =
            root.Q<VisualElement>(
                "HealthFill"
            );

        gameOverContainer =
            root.Q<VisualElement>(
                "GameOverContainer"
            );
    }

    private void Start()
    {
        playerTransformation =
            FindFirstObjectByType<PlayerTransformation>();

        playerStamina =
            FindFirstObjectByType<PlayerStamina>();

        playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        UpdatePortrait();
        UpdateStaminaBar();
        UpdateHealthBar();
    }

    private void Update()
    {
        UpdatePortrait();
        UpdateStaminaBar();
        UpdateHealthBar();
    }

    // =========================================
    // PORTRAIT
    // =========================================

    private void UpdatePortrait()
    {
        if (playerTransformation == null)
            return;

        if (playerTransformation.currentForm ==
            PlayerTransformation.FormState.Human)
        {
            playerFormIcon.style.backgroundImage =
                new StyleBackground(
                    humanPortrait
                );
        }
        else
        {
            playerFormIcon.style.backgroundImage =
                new StyleBackground(
                    wolfPortrait
                );
        }
    }

    // =========================================
    // STAMINA BAR
    // =========================================

    private void UpdateStaminaBar()
    {
        if (playerStamina == null ||
            staminaFill == null)
        {
            return;
        }

        float staminaPercent =
            playerStamina.GetStaminaPercent();

        staminaFill.style.width =
            Length.Percent(
                staminaPercent * 100f
            );
    }

    // =========================================
    // HEALTH BAR
    // =========================================

    private void UpdateHealthBar()
    {
        if (playerHealth == null ||
            healthFill == null)
        {
            return;
        }

        float healthPercent =
            playerHealth.CurrentHealth /
            playerHealth.MaxHealth;

        healthFill.style.width =
            Length.Percent(
                healthPercent * 100f
            );
    }

    // =========================================
    // GAME OVER
    // =========================================

    public void ShowGameOver()
    {
        if (gameOverContainer == null)
            return;

        gameOverContainer.style.display =
            DisplayStyle.Flex;
    }
}