using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]

    [SerializeField]
    private TutorialState requiredState;

    [SerializeField]
    private TutorialState nextState;

    [SerializeField]
    private bool disableAfterTrigger = true;

    [SerializeField]
    private bool changeTutorialState = true;

    private void Reset()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.Instance == null)
            return;

        if (!TutorialManager.Instance.IsCurrentState(requiredState))
            return;

        if (changeTutorialState)
        {
            TutorialManager.Instance.SetState(nextState);
        }

        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}