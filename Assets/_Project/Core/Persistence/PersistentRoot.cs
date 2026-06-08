using UnityEngine;

public class PersistentRoot : MonoBehaviour
{
    private static PersistentRoot instance;

    public static PersistentRoot Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void DestroyPersistentSystems()
    {
        if (instance == null)
            return;

        Destroy(instance.gameObject);
    }
}