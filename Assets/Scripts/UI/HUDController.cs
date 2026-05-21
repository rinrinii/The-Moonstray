using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite humanPortrait;
    public Sprite wolfPortrait;

    private UIDocument uiDocument;

    private VisualElement playerFormIcon;

    private VisualElement staminaFill;

    private PlayerTransformation playerTransformation;
    private PlayerStamina playerStamina;

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
    }

    private void Start()
    {
        playerTransformation =
            FindObjectOfType<PlayerTransformation>();

        playerStamina =
            FindObjectOfType<PlayerStamina>();

        UpdatePortrait();
        UpdateStaminaBar();
    }

    private void Update()
    {
        UpdatePortrait();
        UpdateStaminaBar();
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
}