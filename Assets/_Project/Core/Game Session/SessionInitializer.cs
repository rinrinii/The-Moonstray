using UnityEngine;

public class SessionInitializer : MonoBehaviour
{
    [SerializeField]
    private GameObject persistentSystemsPrefab;

    [SerializeField]
    private Transform initialPersistentSpawn;

    public void CreateSession()
    {
        if (FindFirstObjectByType<PersistentRoot>() != null)
            return;

        if (initialPersistentSpawn != null)
        {
            Instantiate(
                persistentSystemsPrefab,
                initialPersistentSpawn.position,
                initialPersistentSpawn.rotation);
        }
        else
        {
            Instantiate(persistentSystemsPrefab);
        }
    }
}