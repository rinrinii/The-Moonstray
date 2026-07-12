using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpringtideProgressionBootstrap : MonoBehaviour
{
    private const string QuestTitle = "For Every Garden Buries a Secret";
    private const string BloombridgeScene = "Bloombridge Path";
    private const string FarmlandsScene = "Outer Farmlands";
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
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null ||
            !progression.IsAtLeast(GameProgressionStage.Chapter1Spring) ||
            !progression.HasFlag(GameProgressionFlags.Chapter1GuideIntroComplete))
        {
            return;
        }

        if (scene.name == BloombridgeScene)
        {
            if (!progression.HasFlag(
                GameProgressionFlags.Chapter1ArrivedAtBloombridge))
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1ArrivedAtBloombridge);
                SetObjective("Explore Springtide Meadows.");
            }
            return;
        }

        if (scene.name != FarmlandsScene)
            return;

        ConfigureFarmer(progression);
        ConfigureInspection(
            "cabbageRotten1",
            "chapter1.inspectWitheredCrops",
            GameProgressionFlags.Chapter1FarmerIntroComplete,
            GameProgressionFlags.Chapter1WitheredCropsInspected,
            "Examine the damaged irrigation canals.");
        ConfigureInspection(
            "inspectDestroyedIrrigation1",
            "chapter1.inspectDamagedIrrigation",
            GameProgressionFlags.Chapter1WitheredCropsInspected,
            GameProgressionFlags.Chapter1IrrigationInspected,
            "Explore the abandoned greenhouses.");
        ConfigureInspection(
            "inspectAbandonedGreenhouse1",
            "chapter1.inspectAbandonedGreenhouse",
            GameProgressionFlags.Chapter1IrrigationInspected,
            GameProgressionFlags.Chapter1GreenhouseInspected,
            "Visit the Village Basin to obtain more clues.");

        RestoreCurrentObjective(progression);
    }

    private static void ConfigureFarmer(GameProgressionManager progression)
    {
        GameObject farmer = GameObject.Find("FarmerNPC_Male_Rig");
        if (farmer == null)
            return;

        DialogueStageInteraction interaction =
            farmer.GetComponent<DialogueStageInteraction>();
        if (interaction == null)
            return;

        int startingStage = progression.HasFlag(
            GameProgressionFlags.Chapter1FarmerIntroComplete) ? 1 : 0;

        interaction.ConfigureStages(
            new List<DialogueStage>
            {
                new()
                {
                    stageName = "FarmerIntro",
                    dialogueID = "chapter1.farmerIntro",
                    advanceAfterDialogue = true,
                    nextStage = 1,
                    progressionFlagOnComplete =
                        GameProgressionFlags.Chapter1FarmerIntroComplete,
                    objectiveTitleOnComplete = QuestTitle,
                    objectiveDescriptionOnComplete = "Examine the withered crops."
                },
                new()
                {
                    stageName = "FarmerRepeat",
                    dialogueID = "chapter1.farmerRepeat"
                }
            },
            startingStage,
            true);
    }

    private static void ConfigureInspection(
        string objectID,
        string dialogueID,
        string requiredFlag,
        string completionFlag,
        string nextObjective)
    {
        ObjectStateInteraction[] interactions =
            Object.FindObjectsByType<ObjectStateInteraction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (ObjectStateInteraction stateInteraction in interactions)
        {
            if (stateInteraction.ObjectID != objectID)
                continue;

            SpringtideQuestGate gate =
                stateInteraction.GetComponent<SpringtideQuestGate>();
            if (gate == null)
                gate = stateInteraction.gameObject.AddComponent<SpringtideQuestGate>();
            gate.Configure(dialogueID, requiredFlag, completionFlag, nextObjective);
        }
    }

    private static void RestoreCurrentObjective(GameProgressionManager progression)
    {
        if (progression.HasFlag(GameProgressionFlags.Chapter1GreenhouseInspected))
            SetObjective("Visit the Village Basin to obtain more clues.");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1IrrigationInspected))
            SetObjective("Explore the abandoned greenhouses.");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1WitheredCropsInspected))
            SetObjective("Examine the damaged irrigation canals.");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1FarmerIntroComplete))
            SetObjective("Examine the withered crops.");
        else
            SetObjective("Talk to the Farmer.");
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}
