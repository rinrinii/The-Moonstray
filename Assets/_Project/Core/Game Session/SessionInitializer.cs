using UnityEngine;

public class SessionInitializer : MonoBehaviour
{
    [SerializeField]
    private GameObject persistentSystemsPrefab;

    public void CreateSession()
    {
        if (FindFirstObjectByType<PersistentRoot>() != null)
            return;

        Instantiate(persistentSystemsPrefab);
    }
}