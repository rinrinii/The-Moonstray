using UnityEngine;

public class WaningSpawnTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject waningRoot;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (waningRoot != null)
            waningRoot.SetActive(true);
    }
}