using UnityEngine;

public abstract class WaningBase : MonoBehaviour
{
    [Header("Waning Damage")]
    [SerializeField] protected float damagePerSecond = 10f;

    private GameObject affectedPlayer;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        affectedPlayer = other.gameObject;
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

        if (affectedPlayer == other.gameObject)
            affectedPlayer = null;
    }

    protected virtual void OnDisable()
    {
        // Unity does not guarantee OnTriggerExit when a Waning zone is disabled
        // or its scene unloads while the player is still inside it.
        if (affectedPlayer == null)
            return;

        OnPlayerExited(affectedPlayer);
        affectedPlayer = null;
    }

    protected virtual void ApplyDamage(GameObject player)
    {
        float damage = damagePerSecond * Time.deltaTime;

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
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
