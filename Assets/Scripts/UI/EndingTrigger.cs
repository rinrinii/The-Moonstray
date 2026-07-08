using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EndingTrigger : MonoBehaviour
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

        triggered = true;

        GameplayUIManager.Instance.ShowEndingChoice();
    }
}