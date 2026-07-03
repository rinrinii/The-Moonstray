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

        SceneManager.LoadScene("LoadingScene");
    }

    /// <summary>
    /// Scene load with a target spawn point.
    /// Used for region transitions.
    /// </summary>
    public static void LoadScene(string sceneName, string spawnID)
    {
        PendingScene = sceneName;
        PendingSpawnID = spawnID;

        SceneManager.LoadScene("LoadingScene");
    }
}