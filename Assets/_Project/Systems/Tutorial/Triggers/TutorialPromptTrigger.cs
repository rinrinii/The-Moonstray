using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialPromptTrigger : MonoBehaviour
{
    [System.Serializable]
    public enum TriggerMode
    {
        ShowPrompt,
        HidePrompt
    }

    [Header("Prompt")]
    [SerializeField] private TriggerMode mode = TriggerMode.ShowPrompt;

    [SerializeField] private string header;

    [TextArea]
    [SerializeField] private string body;

    [Header("Behaviour")]
    [SerializeField] private bool oneShot = true;

    private bool triggered;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        switch (mode)
        {
            case TriggerMode.ShowPrompt:

                PromptUI.Instance?.Show(
                    header,
                    body
                );

                break;

            case TriggerMode.HidePrompt:

                PromptUI.Instance?.Hide();

                break;
        }

        if (oneShot)
        {
            triggered = true;
            Destroy(gameObject);
        }
    }
}