using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CollapseTrigger : MonoBehaviour
{
    private bool triggered;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        triggered = true;

        playerHealth.SuppressNextGameOver();

        gameObject.SetActive(false);
    }
}