using System.Collections.Generic;
using UnityEngine;

public class FootprintManager : MonoBehaviour
{
    public static FootprintManager Instance;

    [Header("Limit Settings")]
    public int maxFootprints = 20;

    private Queue<GameObject> footprints = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterFootprint(GameObject footprint)
    {
        footprints.Enqueue(footprint);

        if (footprints.Count > maxFootprints)
        {
            GameObject old = footprints.Dequeue();
        }
    }
}