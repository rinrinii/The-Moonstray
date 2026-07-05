using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"Pending Spawn ID = {SceneLoader.PendingSpawnID}");

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        Debug.Log($"Found {spawnPoints.Length} spawn points.");

        foreach (SpawnPoint point in spawnPoints)
        {
            Debug.Log($"SpawnPoint: {point.SpawnID}");

            if (point.SpawnID != SceneLoader.PendingSpawnID)
                continue;

            Debug.Log($"Matched spawn: {point.SpawnID}");

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            Debug.Log(player != null ? "Player found." : "Player NOT found.");

            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();

                if (cc != null)
                    cc.enabled = false;

                player.transform.position = point.transform.position;
                RespawnManager.Instance?.SetCurrentSpawn(point.SpawnID);

                if (cc != null)
                    cc.enabled = true;

                Debug.Log($"Moved player to {player.transform.position}");
            }

            break;
        }

        SceneLoader.PendingSpawnID = null;

        CompleteTutorialIfMoonveilReached();
    }

    private void CompleteTutorialIfMoonveilReached()
    {
        if (SceneManager.GetActiveScene().name != "Moonveil")
            return;

        if (TutorialManager.Instance == null ||
            TutorialManager.Instance.CurrentState != TutorialState.ReadingWing)
        {
            return;
        }

        ObjectivesUI.Instance?.Clear();
        TutorialManager.Instance.FinishTutorial();
    }
}
