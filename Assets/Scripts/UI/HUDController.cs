using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [Header("Portrait Sprites")]
    public Sprite humanPortrait;
    public Sprite wolfPortrait;

    private UIDocument uiDocument;

    private VisualElement playerFormIcon;

    private PlayerTransformation playerTransformation;

    private void Awake()
    {
        uiDocument =
            GetComponent<UIDocument>();

        VisualElement root =
            uiDocument.rootVisualElement;

        playerFormIcon =
            root.Q<VisualElement>("PlayerFormIcon");
    }

    private void Start()
    {
        playerTransformation =
            FindObjectOfType<PlayerTransformation>();

        UpdatePortrait();
    }

    private void Update()
    {
        UpdatePortrait();
    }

    private void UpdatePortrait()
    {
        if (playerTransformation == null)
            return;

        if (playerTransformation.currentForm ==
            PlayerTransformation.FormState.Human)
        {
            playerFormIcon.style.backgroundImage =
                new StyleBackground(humanPortrait);
        }
        else
        {
            playerFormIcon.style.backgroundImage =
                new StyleBackground(wolfPortrait);
        }
    }
}