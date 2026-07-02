using UnityEngine;

public class FrostmereLibraryController : MonoBehaviour
{
    private void Start()
    {
        PlayerHealth playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ReviveAtFullHealth();
        }

        if (TutorialManager.Instance == null)
            return;

        if (!TutorialManager.Instance.IsTutorialFinished)
        {
            TutorialManager.Instance.SetStep(
                TutorialStep.NPCArrival
            );
        }
    }
}