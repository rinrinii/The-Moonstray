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
                player.transform.rotation = point.transform.rotation;
                RespawnManager.Instance?.SetCurrentSpawn(point.SpawnID);

                if (cc != null)
                    cc.enabled = true;

                ResetPlayerTeleportState(player);

                Debug.Log($"Moved player to {player.transform.position}");
            }

            break;
        }

        SceneLoader.PendingSpawnID = null;

        UpdatePostLibraryTutorialObjective();
        CompleteTutorialIfMoonveilReached();
    }

    private void UpdatePostLibraryTutorialObjective()
    {
        if (TutorialManager.Instance == null ||
            TutorialManager.Instance.CurrentState != TutorialState.ReadingWing)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Frostmere Library" || sceneName == "Moonveil")
            return;

        ObjectivesUI.Instance?.SetObjective(
            "tutorial.leaving_the_past_behind",
            "travel_moonveil",
            0);
    }

    private void ResetPlayerTeleportState(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement?.ResetVerticalVelocity();

        FallDamage fallDamage = player.GetComponent<FallDamage>();
        fallDamage?.ResetFallTracking();
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

        if (GameProgressionManager.Instance != null)
        {
            GameProgressionManager.Instance.CompleteTutorialAndBeginChapterOne();
            MoonveilProgressionBootstrap.RefreshQuestObjective();
        }
        else
        {
            TutorialManager.Instance.FinishTutorial();
            FindFirstObjectByType<PlayerHealth>()?.RestoreFullHealth();

            ObjectivesUI.Instance?.SetObjective(
                "chapter1.new_beginnings",
                "talk_to_guide",
                0);
        }
    }
}
