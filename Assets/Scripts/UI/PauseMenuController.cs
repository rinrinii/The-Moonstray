using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private UIDocument uiDocument;

    private VisualElement pauseContainer;
    private VisualElement hudContainer;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        uiDocument = GetComponent<UIDocument>();

        VisualElement root =
            uiDocument.rootVisualElement;

        pauseContainer =
            root.Q<VisualElement>("PauseContainer");

        hudContainer =
            root.Q<VisualElement>("HUDContainer");

        // Hide pause menu on start
        pauseContainer.style.display =
            DisplayStyle.None;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        // Show pause menu
        pauseContainer.style.display =
            DisplayStyle.Flex;

        // Hide HUD
        if (hudContainer != null)
        {
            hudContainer.style.display =
                DisplayStyle.None;
        }

        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        // Hide pause menu
        pauseContainer.style.display =
            DisplayStyle.None;

        // Show HUD again
        if (hudContainer != null)
        {
            hudContainer.style.display =
                DisplayStyle.Flex;
        }

        Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        // Safety reset in case object gets disabled while paused
        Time.timeScale = 1f;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}