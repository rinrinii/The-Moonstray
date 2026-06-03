using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    private void Start()
    {
        if (string.IsNullOrEmpty(
            SceneLoader.PendingSpawnID))
            return;

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(
                FindObjectsSortMode.None
            );

        foreach (SpawnPoint point
            in spawnPoints)
        {
            if (point.SpawnID !=
                SceneLoader.PendingSpawnID)
                continue;

            GameObject player =
                GameObject.FindGameObjectWithTag(
                    "Player"
                );

            if (player != null)
            {
                player.transform.position =
                    point.transform.position;
            }

            break;
        }

        SceneLoader.PendingSpawnID =
            null;
    }
}