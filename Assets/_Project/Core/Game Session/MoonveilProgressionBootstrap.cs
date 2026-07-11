using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoonveilProgressionBootstrap : MonoBehaviour
{
    private const string MoonveilSceneName = "Moonveil";
    private static bool registered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        if (!registered)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            registered = true;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid())
            ConfigureScene(activeScene);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureScene(scene);
    }

    private static void ConfigureScene(Scene scene)
    {
        if (scene.name != MoonveilSceneName)
            return;

        ConfigureGuide();
        ConfigureQuestObjective();
        ConfigureBlocker(
            "SpringExitBlocker",
            GameProgressionStage.Chapter1Spring,
            GameProgressionFlags.Chapter1GuideIntroComplete,
            "chapter1.blockedSpringExit");
        ConfigureBlocker(
            "SummerExitBlocker",
            GameProgressionStage.Chapter2Summer,
            "",
            "progression.blockedExit");
        ConfigureBlocker(
            "AutumnExitBlocker",
            GameProgressionStage.Chapter3Autumn,
            "",
            "progression.blockedExit");
        ConfigureBlocker(
            "WinterExitBlocker",
            GameProgressionStage.Chapter4Winter,
            "",
            "progression.blockedExit");
    }

    private static void ConfigureGuide()
    {
        GameObject guide = GameObject.Find("Guide NPC-Rig");
        if (guide == null)
            return;

        DialogueStageInteraction interaction =
            guide.GetComponentInChildren<DialogueStageInteraction>(true);

        if (interaction == null)
            return;

        interaction.ConfigureStages(
            new List<DialogueStage>
            {
                new()
                {
                    stageName = "GuideIntro",
                    dialogueID = "chapter1.guideIntro",
                    advanceAfterDialogue = true,
                    nextStage = 1,
                    progressionFlagOnComplete =
                        GameProgressionFlags.Chapter1GuideIntroComplete,
                    objectiveTitleOnComplete = "New Beginnings",
                    objectiveDescriptionOnComplete =
                        "Travel to Springtide Meadows."
                },
                new()
                {
                    stageName = "GuideIdle1",
                    dialogueID = "chapter1.guideIdle1",
                    advanceAfterDialogue = true,
                    nextStage = 2
                },
                new()
                {
                    stageName = "GuideIdle2",
                    dialogueID = "chapter1.guideIdle2"
                }
            });
    }

    private static void ConfigureQuestObjective()
    {
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            !progression.IsAtLeast(GameProgressionStage.Chapter1Spring) ||
            progression.HasFlag(GameProgressionFlags.Chapter1GuideIntroComplete))
        {
            return;
        }

        ObjectivesUI.Instance?.SetObjective(
            "New Beginnings",
            "Talk to the Guide.");
    }

    private static void ConfigureBlocker(
        string blockerName,
        GameProgressionStage unlockStage,
        string requiredFlag,
        string blockedDialogueID)
    {
        GameObject blocker = GameObject.Find(blockerName);
        if (blocker == null)
            return;

        ProgressionExitBlocker progressionBlocker =
            blocker.GetComponent<ProgressionExitBlocker>();

        if (progressionBlocker == null)
            progressionBlocker = blocker.AddComponent<ProgressionExitBlocker>();

        progressionBlocker.Configure(unlockStage, requiredFlag);

        TutorialExitBlockerDialogue[] dialogues =
            blocker.GetComponentsInChildren<TutorialExitBlockerDialogue>(true);

        foreach (TutorialExitBlockerDialogue dialogue in dialogues)
            dialogue.SetDialogueID(blockedDialogueID);
    }
}
