using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial")]

    [SerializeField]
    private TutorialStep requiredStep;

    [SerializeField]
    private bool completeStepOnEnter = true;

    [SerializeField]
    private bool disableAfterTrigger = true;

    private void Reset()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (TutorialManager.Instance == null)
            return;

        if (!TutorialManager.Instance.IsCurrentStep(requiredStep))
            return;

        if (completeStepOnEnter)
            TutorialManager.Instance.CompleteCurrentStep();

        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}