using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string PendingScene;
    public static string PendingSpawnID;

    /// <summary>
    /// Standard scene load without a spawn point.
    /// Used for menus and other non-world transitions.
    /// </summary>

    public static void LoadScene(string sceneName)
    {
        PendingScene = sceneName;
        PendingSpawnID = null;

        BeginTransition(sceneName);
    }

    /// <summary>
    /// Scene load with a target spawn point.
    /// Used for region transitions.
    /// </summary>
    public static void LoadScene(string sceneName, string spawnID)
    {
        PendingScene = sceneName;
        PendingSpawnID = spawnID;

        BeginTransition(sceneName);
    }

    private static void BeginTransition(string sceneName)
    {
        ClearTransientPlayerEffects();
        GameplayUIManager.Instance?.SetGameplayUIVisible(false);

        bool leavingMainMenu =
            SceneManager.GetActiveScene().name == "MainMenu" &&
            sceneName != "MainMenu";

        if (leavingMainMenu)
        {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        SceneTransitionRunner.Begin(sceneName);
    }

    private static void ClearTransientPlayerEffects()
    {
        StatusEffectManager.Instance?.ClearAll();
        PlayerTransformation.Instance?.SetSpeedModifier(1f);
    }
}
