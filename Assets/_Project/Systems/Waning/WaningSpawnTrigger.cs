using UnityEngine;

public class WaningSpawnTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject waning;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        if (waning != null)
            waning.SetActive(true);
    }
}