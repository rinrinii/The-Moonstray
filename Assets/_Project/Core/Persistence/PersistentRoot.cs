using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentRoot : MonoBehaviour
{
    private static PersistentRoot instance;

    public static PersistentRoot Instance => instance;

    private Camera gameplayCamera;
    private AudioListener gameplayAudioListener;
    private GameObject player;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        gameplayCamera = GetComponentInChildren<Camera>(true);

        if (gameplayCamera != null)
            gameplayAudioListener = gameplayCamera.GetComponent<AudioListener>();

        PlayerMovement playerMovement =
            GetComponentInChildren<PlayerMovement>(true);

        if (playerMovement != null)
            player = playerMovement.gameObject;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        ApplySceneMode(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (instance == this)
        {
            instance = null;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"PersistentRoot: Scene Loaded -> {scene.name}");
        ApplySceneMode(scene);
    }

    private void ApplySceneMode(Scene scene)
    {
        bool isMainMenu = scene.name == "MainMenu";

        if (gameplayCamera != null)
            gameplayCamera.enabled = !isMainMenu;

        if (gameplayAudioListener != null)
            gameplayAudioListener.enabled = !isMainMenu;
    }

    public static void DestroyPersistentSystems()
    {
        if (instance == null)
            return;

        Destroy(instance.gameObject);
    }
}
