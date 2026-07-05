using UnityEngine;
using UnityEngine.UIElements;

public class GameOverUI : MonoBehaviour
{
    private VisualElement gameOverRoot;

    private Button retryButton;
    private Button mainMenuButton;
    private Button exitButton;

    private void Start()
    {
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        GameplayUIManager ui =
            GameplayUIManager.Instance;

        if (ui == null)
            return;

        gameOverRoot = ui.GameOverContainer;

        if (gameOverRoot == null)
            return;

        retryButton =
            gameOverRoot.Q<Button>("RetryButton");

        mainMenuButton =
            gameOverRoot.Q<Button>("MainMenuButton");

        exitButton =
            gameOverRoot.Q<Button>("ExitButton");

        if (retryButton != null)
            retryButton.clicked += Retry;

        if (mainMenuButton != null)
            mainMenuButton.clicked += ReturnToMainMenu;

        if (exitButton != null)
            exitButton.clicked += ExitGame;

        Hide();
    }

    private void OnDestroy()
    {
        if (retryButton != null)
            retryButton.clicked -= Retry;

        if (mainMenuButton != null)
            mainMenuButton.clicked -= ReturnToMainMenu;

        if (exitButton != null)
            exitButton.clicked -= ExitGame;
    }

    public void Show()
    {
        if (gameOverRoot == null)
            return;

        gameOverRoot.style.display =
            DisplayStyle.Flex;

        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (gameOverRoot == null)
            return;

        gameOverRoot.style.display =
            DisplayStyle.None;

        Time.timeScale = 1f;
    }

    private void Retry()
    {
        Debug.Log("Retry pressed.");

        Hide();

        RespawnManager.Instance?.Respawn();
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Main Menu pressed.");

        // TODO
    }

    private void ExitGame()
    {
        Debug.Log("Exit pressed.");

        Application.Quit();
    }
}