using UnityEngine;

public abstract class WaningBase : MonoBehaviour
{
    [Header("Waning Damage")]
    [SerializeField] protected float damagePerSecond = 10f;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        OnPlayerEntered(other.gameObject);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ApplyDamage(other.gameObject);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        OnPlayerExited(other.gameObject);
    }

    private float logTimer;
    [SerializeField] private float logInterval = 1f;

    protected virtual void ApplyDamage(GameObject player)
    {
        float damage = damagePerSecond * Time.deltaTime;

        // Apply actual damage here later

        logTimer += Time.deltaTime;

        if (logTimer >= logInterval)
        {
            Debug.Log($"Player is taking Waning damage.");
            logTimer = 0f;
        }
    }

    // =========================
    // Custom behavior hooks
    // =========================

    protected virtual void OnPlayerEntered(GameObject player)
    {

    }

    protected virtual void OnPlayerExited(GameObject player)
    {

    }
}