using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static string PendingSpawnID;

    public static void LoadScene(
        string sceneName,
        string spawnID)
    {
        PendingSpawnID = spawnID;

        SceneManager.LoadScene(sceneName);
    }
}