using System.Collections;
using UnityEngine;

public class FrostmereLibraryController : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField]
    private GameObject npcModel;

    [Header("Dialogue")]
    [SerializeField]
    private string introDialogue = "intro.leaveForSupplies";

    [SerializeField]
    private string returnDialogue = "intro.studentReaction";

    private PlayerTransformation playerTransformation;

    private void Start()
    {
        PlayerHealth playerHealth =
            FindFirstObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.ReviveAtFullHealth();
        }

        npcModel?.SetActive(true);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                introDialogue);
        }

        StartCoroutine(BeginLibraryTutorial());
    }

    private IEnumerator BeginLibraryTutorial()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsDialogueActive());

        if (npcModel != null)
        {
            npcModel.SetActive(false);
        }

        playerTransformation =
            FindFirstObjectByType<PlayerTransformation>();

        if (playerTransformation != null)
        {
            playerTransformation.UnlockTransformation();

            playerTransformation.OnTransformationComplete +=
                HandleTransformation;
        }

        GameplayUIManager.Instance.Prompt.Show(
            "[ F ] Transform",
            "Transform into your human form."
        );

        QuestManager.Instance?.StartQuest(
            "While Waiting",
            new string[]
            {
                "Transform into Human",
                "Examine Notes"
            });

        TutorialManager.Instance?.SetStep(
            TutorialStep.Transform);
    }

    private void HandleTransformation(
        PlayerTransformation.FormState form)
    {
        if (form != PlayerTransformation.FormState.Human)
            return;

        GameplayUIManager.Instance.Prompt.Hide();

        TutorialManager.Instance?.CompleteCurrentStep();

        GameplayUIManager.Instance.Prompt.Show(
            "[ E ] Interact",
            "Examine the nearby notes."
        );
    }

    private void OnDestroy()
    {
        if (playerTransformation != null)
        {
            playerTransformation.OnTransformationComplete -=
                HandleTransformation;
        }
    }
}