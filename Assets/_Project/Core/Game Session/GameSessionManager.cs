using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSessionManager
{
    [Serializable]
    public sealed class Snapshot
    {
        public int schemaVersion = CurrentSchemaVersion;
        public GameProgressionManager.Snapshot progression;
        public QuestManager.Snapshot quests;
        public TutorialState tutorialState;
        public InventorySystem.Snapshot inventory;
        public int moonCoins = MoonCoinWallet.StartingMoonCoins;
        public List<AbilityType> abilities = new();
        public List<UpgradeType> upgrades = new();
        public JournalController.Snapshot journal;
        public bool inventoryUnlocked;
        public bool mapUnlocked;
        public WorldSnapshot world;
    }

    [Serializable]
    public sealed class WorldSnapshot
    {
        public string sceneName;
        public string spawnID;
        public bool hasPlayerState;
        public Vector3 position;
        public Quaternion rotation;
        public float health;
        public float stamina;
        public PlayerTransformation.FormState form;
        public bool transformationUnlocked;
    }

    public const int CurrentSchemaVersion = 3;

    private static GameSessionRestoreRunner restoreRunner;

    public static Snapshot CaptureState()
    {
        return new Snapshot
        {
            progression = GameProgressionManager.Instance?.CaptureState(),
            quests = QuestManager.Instance?.CaptureState(),
            tutorialState = TutorialManager.Instance != null
                ? TutorialManager.Instance.CaptureState()
                : TutorialState.None,
            inventory = InventorySystem.Instance?.CaptureState(),
            moonCoins = MoonCoinWallet.Instance != null
                ? MoonCoinWallet.Instance.MoonCoins
                : MoonCoinWallet.StartingMoonCoins,
            abilities = AbilityManager.Instance?.CaptureState() ?? new List<AbilityType>(),
            upgrades = UpgradeManager.Instance?.CaptureState() ?? new List<UpgradeType>(),
            journal = JournalController.Instance?.CaptureState(),
            inventoryUnlocked = InventoryUI.Instance != null && InventoryUI.Instance.IsUnlocked,
            mapUnlocked = GameplayUIManager.Instance?.Map != null && GameplayUIManager.Instance.Map.HasMap,
            world = CaptureWorldState()
        };
    }

    public static void RestoreState(Snapshot snapshot)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("Game session restore ignored an empty snapshot.");
            return;
        }

        if (snapshot.schemaVersion > CurrentSchemaVersion)
            Debug.LogWarning($"Save schema {snapshot.schemaVersion} is newer than supported schema {CurrentSchemaVersion}.");

        // Inventory is restored before quests because quest requirements may query it.
        InventorySystem.Instance?.RestoreState(snapshot.inventory);
        MoonCoinWallet.Instance?.SetAmount(snapshot.moonCoins);
        AbilityManager.Instance?.RestoreState(snapshot.abilities);
        UpgradeManager.Instance?.RestoreState(snapshot.upgrades);
        GameProgressionManager.Instance?.RestoreState(snapshot.progression);
        QuestManager.Instance?.RestoreState(snapshot.quests);
        JournalController.Instance?.RestoreState(snapshot.journal);
        bool inventoryUnlocked = snapshot.inventoryUnlocked;
        bool mapUnlocked = snapshot.mapUnlocked;
        if (snapshot.schemaVersion < 3)
        {
            inventoryUnlocked = snapshot.tutorialState >= TutorialState.WakeInLibrary;
            mapUnlocked = snapshot.tutorialState >= TutorialState.ReadingWing;
        }

        InventoryUI.Instance?.RestoreUnlockState(inventoryUnlocked);
        GameplayUIManager.Instance?.Map?.RestoreUnlockState(mapUnlocked);

        // Tutorial listeners can inspect restored quest state when this fires.
        TutorialManager.Instance?.RestoreState(snapshot.tutorialState);

        RestoreWorldState(snapshot.world);
    }

    public static void ResetProgressForNewGame()
    {
        InventorySystem.Instance?.ResetState();
        MoonCoinWallet.Instance?.ResetState();
        JournalController.Instance?.ResetState();
        InventoryUI.Instance?.RestoreUnlockState(false);
        GameplayUIManager.Instance?.Map?.RestoreUnlockState(false);
        QuestManager.Instance?.ResetProgress();
        GameProgressionManager.Instance?.ResetProgress();
        TutorialManager.Instance?.ResetProgress();

        AbilityManager.Instance?.LockAllAbilities();
        AbilityManager.Instance?.UnlockAbility(AbilityType.Climb);
        UpgradeManager.Instance?.LockAllUpgrades();
        RespawnManager.Instance?.ClearCurrentSpawn();
    }

    private static WorldSnapshot CaptureWorldState()
    {
        Scene scene = SceneManager.GetActiveScene();
        PlayerHealth health = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        PlayerStamina stamina = health != null
            ? health.GetComponent<PlayerStamina>()
            : UnityEngine.Object.FindFirstObjectByType<PlayerStamina>();
        PlayerTransformation transformation = health != null
            ? health.GetComponent<PlayerTransformation>()
            : PlayerTransformation.Instance;

        WorldSnapshot world = new()
        {
            sceneName = scene.IsValid() ? scene.name : string.Empty,
            spawnID = RespawnManager.Instance?.CurrentSpawnID,
            hasPlayerState = health != null,
            health = health != null ? health.CurrentHealth : 0f,
            stamina = stamina != null ? stamina.GetCurrentStamina() : 0f,
            form = transformation != null
                ? transformation.currentForm
                : PlayerTransformation.FormState.Wolf,
            transformationUnlocked = transformation != null && transformation.CanTransform
        };

        if (health != null)
        {
            world.position = health.transform.position;
            world.rotation = health.transform.rotation;
        }

        return world;
    }

    private static void RestoreWorldState(WorldSnapshot world)
    {
        if (world == null || !world.hasPlayerState)
            return;

        EnsureRestoreRunner();
        if (restoreRunner == null)
        {
            Debug.LogWarning("Player state could not be scheduled for restoration.");
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrWhiteSpace(world.sceneName) && world.sceneName != activeScene)
        {
            if (!Application.CanStreamedLevelBeLoaded(world.sceneName))
            {
                Debug.LogWarning($"Saved scene '{world.sceneName}' is unavailable. Keeping the current scene.");
                restoreRunner.RestoreAfterSceneReady(world);
                return;
            }

            restoreRunner.WaitForSceneAndRestore(world);
            SceneLoader.LoadScene(world.sceneName, world.spawnID);
            return;
        }

        restoreRunner.RestoreAfterSceneReady(world);
    }

    private static void EnsureRestoreRunner()
    {
        if (restoreRunner != null)
            return;

        GameObject host = PersistentRoot.Instance != null
            ? PersistentRoot.Instance.gameObject
            : new GameObject(nameof(GameSessionRestoreRunner));

        restoreRunner = host.GetComponent<GameSessionRestoreRunner>();
        if (restoreRunner == null)
            restoreRunner = host.AddComponent<GameSessionRestoreRunner>();

        if (PersistentRoot.Instance == null)
            UnityEngine.Object.DontDestroyOnLoad(host);
    }

    internal static void ApplyPlayerState(WorldSnapshot world)
    {
        PlayerHealth health = UnityEngine.Object.FindFirstObjectByType<PlayerHealth>();
        if (health == null)
        {
            Debug.LogWarning("Saved player state could not be restored because no player was found.");
            return;
        }

        PlayerStamina stamina = health.GetComponent<PlayerStamina>();
        PlayerTransformation transformation = health.GetComponent<PlayerTransformation>();

        if (string.IsNullOrWhiteSpace(world.spawnID))
            RespawnManager.Instance?.ClearCurrentSpawn();
        else
            RespawnManager.Instance?.SetCurrentSpawn(world.spawnID);

        if (IsFinite(world.position) && IsValid(world.rotation))
        {
            CharacterController controller = health.GetComponent<CharacterController>();
            if (controller != null)
                controller.enabled = false;

            health.transform.SetPositionAndRotation(world.position, world.rotation);

            if (controller != null)
                controller.enabled = true;

            health.GetComponent<PlayerMovement>()?.ResetVerticalVelocity();
            health.GetComponent<FallDamage>()?.ResetFallTracking();
        }

        health.RestoreState(world.health);
        stamina?.RestoreState(stamina.GetMaxStamina());

        if (transformation != null)
        {
            if (world.form == PlayerTransformation.FormState.Human)
                transformation.ForceHumanForm();
            else
                transformation.ForceWolfForm();

            if (world.transformationUnlocked)
                transformation.UnlockTransformation();
            else
                transformation.LockTransformation();
        }


        ObjectivesUI.Instance?.RefreshDisplayedQuest();
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }

    private static bool IsValid(Quaternion value)
    {
        return float.IsFinite(value.x) && float.IsFinite(value.y) &&
            float.IsFinite(value.z) && float.IsFinite(value.w) &&
            value.x * value.x + value.y * value.y +
            value.z * value.z + value.w * value.w > 0.0001f;
    }
}

public sealed class GameSessionRestoreRunner : MonoBehaviour
{
    private GameSessionManager.WorldSnapshot pendingWorld;

    public void WaitForSceneAndRestore(GameSessionManager.WorldSnapshot world)
    {
        pendingWorld = world;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public void RestoreAfterSceneReady(GameSessionManager.WorldSnapshot world)
    {
        StartCoroutine(RestoreNextFrame(world));
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingWorld == null || scene.name != pendingWorld.sceneName)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        GameSessionManager.WorldSnapshot world = pendingWorld;
        pendingWorld = null;
        StartCoroutine(RestoreNextFrame(world));
    }

    private IEnumerator RestoreNextFrame(GameSessionManager.WorldSnapshot world)
    {
        // Player components initialize their default values in Start().
        yield return null;
        GameSessionManager.ApplyPlayerState(world);
        yield return null;
        ObjectivesUI.Instance?.RefreshDisplayedQuest();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
}
