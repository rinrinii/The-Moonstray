using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance
    {
        get;
        private set;
    }

    private string currentSpawnID;
    public string CurrentSpawnID => currentSpawnID;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            ReturnToCurrentSpawn();
    }

    // =========================================
    // CURRENT SPAWN
    // =========================================

    /// <summary>
    /// Updates the player's current respawn point.
    /// Called whenever the player is spawned into
    /// a scene or reaches a new checkpoint.
    /// </summary>
    public void SetCurrentSpawn(string spawnID)
    {
        Debug.Log($"Current spawn = {spawnID}");
        if (string.IsNullOrWhiteSpace(spawnID))
            return;

        currentSpawnID = spawnID;

        Debug.Log($"Respawn point updated: {spawnID}");
    }

    public void ClearCurrentSpawn()
    {
        currentSpawnID = null;
    }

    // =========================================
    // RESPAWN
    // =========================================

    /// <summary>
    /// Restores the player and returns them to
    /// their latest recorded spawn point.
    /// </summary>
    public void Respawn()
    {
        if (string.IsNullOrWhiteSpace(currentSpawnID))
        {
            Debug.LogWarning("RespawnManager: No respawn point set.");
            return;
        }

        Debug.Log("Respawn called.");

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(
                FindObjectsSortMode.None);

        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.SpawnID != currentSpawnID)
                continue;

            PlayerHealth playerHealth = FindPlayer();

            if (playerHealth == null)
            {
                Debug.LogWarning("RespawnManager: Player not found.");
                return;
            }

            CharacterController cc =
                playerHealth.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            playerHealth.transform.position =
                point.transform.position;

            playerHealth.transform.rotation =
                point.transform.rotation;

            if (cc != null)
                cc.enabled = true;

            ResetPlayerTeleportState(playerHealth.gameObject);

            // Restore gameplay state.
            playerHealth.RestoreFullHealth();

            StatusEffectManager.Instance?.ClearAll();

            GameplayUIManager.Instance?.GameOver?.Hide();

            Debug.Log(
                $"Player respawned at '{currentSpawnID}'.");

            return;
        }

        Debug.LogWarning(
            $"RespawnManager: Spawn '{currentSpawnID}' not found.");
    }

    /// <summary>
    /// Unstuck shortcut: returns the player to the latest recorded spawn
    /// without healing or clearing gameplay status effects.
    /// </summary>
    public void ReturnToCurrentSpawn()
    {
        if (string.IsNullOrWhiteSpace(currentSpawnID))
        {
            Debug.LogWarning("Unstuck: No spawn point has been recorded yet.");
            return;
        }

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint point in spawnPoints)
        {
            if (point.SpawnID != currentSpawnID)
                continue;

            PlayerHealth playerHealth = FindPlayer();
            if (playerHealth == null)
            {
                Debug.LogWarning("Unstuck: Player not found.");
                return;
            }

            CharacterController controller =
                playerHealth.GetComponent<CharacterController>();

            if (controller != null)
                controller.enabled = false;

            playerHealth.transform.SetPositionAndRotation(
                point.transform.position,
                point.transform.rotation);

            if (controller != null)
                controller.enabled = true;

            ResetPlayerTeleportState(playerHealth.gameObject);

            Debug.Log(
                $"Unstuck: Returned player to '{currentSpawnID}'.");
            return;
        }

        Debug.LogWarning(
            $"Unstuck: Spawn '{currentSpawnID}' was not found in this scene.");
    }

    // =========================================
    // HELPERS
    // =========================================

    private PlayerHealth FindPlayer()
    {
        return FindFirstObjectByType<PlayerHealth>();
    }

    private void ResetPlayerTeleportState(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement?.ResetVerticalVelocity();

        FallDamage fallDamage = player.GetComponent<FallDamage>();
        fallDamage?.ResetFallTracking();
    }
}
