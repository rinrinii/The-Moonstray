using UnityEngine;

public class TutorialBootstrap : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("TutorialBootstrap Start");

        Debug.Log("TutorialManager = " + TutorialManager.Instance);

        if (TutorialManager.Instance == null)
        {
            Debug.Log("No TutorialManager");
            return;
        }

        Debug.Log("ShouldStartTutorial = " +
                  TutorialManager.Instance.ShouldStartTutorial);

        if (!TutorialManager.Instance.ShouldStartTutorial)
            return;

        Debug.Log("Starting tutorial");

        TutorialManager.Instance.StartTutorial();
    }
}