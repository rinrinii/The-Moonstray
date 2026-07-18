using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpringtideProgressionBootstrap : MonoBehaviour
{
    private const string QuestTitle = "For Every Garden Buries a Secret";
    private const string BloombridgeScene = "Bloombridge Path";
    private const string FarmlandsScene = "Outer Farmlands";
    private const string VillageBasinScene = "Village Basin";
    private const string OvergrowthFieldsScene = "Overgrowth Fields";
    private const string ViridianEstateScene = "Viridian Estate";
    private const string RestrictedFarmlandsScene = "Restricted Farmlands";
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

        // Install Overgrowth Fields response components even when this scene
        // is opened directly in the editor or progression finishes loading a
        // little later. Each response still enforces its own quest gates.
        if (scene.name == OvergrowthFieldsScene)
        {
            ConfigureOvergrowthFields(progression);
            return;
        }

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
                ObjectivesUI.Instance?.SetObjective(
                    "chapter1.new_beginnings",
                    "visit_outer_farmlands",
                    0);
            }
            return;
        }

        if (scene.name == VillageBasinScene)
        {
            ConfigureVillageBasin(progression);
            return;
        }

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1VerdantShardObtained))
        {
            AbilityManager.Instance?.UnlockAbility(AbilityType.Dash);
        }

        if (scene.name == ViridianEstateScene)
        {
            ConfigureViridianEstate(progression);
            return;
        }


        if (scene.name == RestrictedFarmlandsScene)
        {
            ConfigureRestrictedFarmlands(progression);
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

    private static void ConfigureViridianEstate(
        GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropTwoInspected))
        {
            return;
        }

        GameObject viridian = GameObject.Find("Viridian-rig");
        if (viridian == null)
            return;

        DialogueStageInteraction interaction =
            viridian.GetComponent<DialogueStageInteraction>();
        if (interaction == null)
            return;

        int startingStage = progression.HasFlag(
            GameProgressionFlags.Chapter1ViridianIntroComplete) ? 1 : 0;

        interaction.ConfigureStages(
            new List<DialogueStage>
            {
                new()
                {
                    stageName = "ViridianIntro",
                    dialogueID = "chapter1.viridianIntro",
                    advanceAfterDialogue = true,
                    nextStage = 1,
                    progressionFlagOnComplete =
                        GameProgressionFlags.Chapter1ViridianIntroComplete,
                    objectiveTitleOnComplete = QuestTitle,
                    objectiveDescriptionOnComplete =
                        "Proceed to the Restricted Farmlands."
                },
                new()
                {
                    stageName = "ViridianRepeat",
                    dialogueID = "chapter1.viridianRepeat"
                }
            },
            startingStage,
            true);

        if (startingStage == 1)
            SetObjective("Proceed to the Restricted Farmlands.");
        else
            SetObjective("Speak with Viridian, the Harvest Steward.");
    }

    private static void ConfigureRestrictedFarmlands(
        GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1ViridianIntroComplete))
        {
            return;
        }

        ObjectStateInteraction[] interactions =
            Object.FindObjectsByType<ObjectStateInteraction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (ObjectStateInteraction stateInteraction in interactions)
        {
            if (stateInteraction.ObjectID != "irrigationWheel")
                continue;

            RestrictedFarmlandsQuestInteraction quest =
                stateInteraction.GetComponent<
                    RestrictedFarmlandsQuestInteraction>();
            if (quest == null)
            {
                quest = stateInteraction.gameObject.AddComponent<
                    RestrictedFarmlandsQuestInteraction>();
            }

            GameObject water = GameObject.Find("Water");
            quest.Configure(
                water,
                Resources.Load<ItemData>("Items/Fragments/Verdant Shard"));
            break;
        }

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1VerdantShardObtained))
        {
            waterStateObjectiveComplete();
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1RestrictedWheelInspected))
        {
            SetObjective("Interact with the irrigation wheel again.");
        }
        else
        {
            SetObjective("Interact with the irrigation wheel.");
        }

        static void waterStateObjectiveComplete()
        {
            SetObjective("The truth beneath the farmland has been revealed.");
        }
    }

    private static void ConfigureOvergrowthFields(
        GameProgressionManager progression)
    {
        ObjectStateInteraction[] interactions =
            Object.FindObjectsByType<ObjectStateInteraction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (ObjectStateInteraction stateInteraction in interactions)
        {
            if (stateInteraction.ObjectID == "inspectRottenCrop2")
            {
                ConfigureOvergrowthInteraction(
                    stateInteraction.gameObject,
                    OvergrowthFieldsQuestInteraction.Step.CropOne);
            }
            else if (stateInteraction.ObjectID == "inspectRottenCrop3")
            {
                ConfigureOvergrowthInteraction(
                    stateInteraction.gameObject,
                    OvergrowthFieldsQuestInteraction.Step.CropTwo);
            }
        }

        if (progression != null)
            RestoreOvergrowthObjective(progression);
    }

    private static void ConfigureOvergrowthInteraction(
        GameObject target,
        OvergrowthFieldsQuestInteraction.Step step)
    {
        OvergrowthFieldsQuestInteraction interaction =
            target.GetComponent<OvergrowthFieldsQuestInteraction>();
        if (interaction == null)
            interaction = target.AddComponent<OvergrowthFieldsQuestInteraction>();

        interaction.Configure(step);
    }

    private static void RestoreOvergrowthObjective(
        GameProgressionManager progression)
    {
        if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropTwoInspected))
        {
            SetObjective("Look for the Harvest Steward of Springtide Meadows.");
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
        {
            SetObjective("Inspect the rotting crops. (1/2)");
        }
        else
        {
            SetObjective("Inspect the rotting crops. (0/2)");
        }
    }

    private static void ConfigureVillageBasin(
        GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1GreenhouseInspected))
        {
            return;
        }

        ObjectStateInteraction[] interactions =
            Object.FindObjectsByType<ObjectStateInteraction>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (ObjectStateInteraction stateInteraction in interactions)
        {
            if (stateInteraction.ObjectID != "restoreDestroyedIrrigation2")
                continue;

            RestoreBehaviour restore =
                stateInteraction.GetComponent<RestoreBehaviour>();
            if (restore == null)
                continue;

            restore.PrepareRuntimeReplacement();

            VillageBasinIrrigationQuest quest =
                stateInteraction.GetComponent<VillageBasinIrrigationQuest>();
            if (quest == null)
            {
                quest = stateInteraction.gameObject.AddComponent<
                    VillageBasinIrrigationQuest>();
            }

            quest.Configure(restore, Resources.Load<ItemData>("Items/Shovel"));
            break;
        }
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

public class RestrictedFarmlandsQuestInteraction : MonoBehaviour,
    IInteractionResponse
{
    private const string QuestTitle = "For Every Garden Buries a Secret";
    private GameObject water;
    private ItemData verdantShard;
    private bool configured;
    private bool transitioning;

    public void Configure(
        GameObject configuredWater,
        ItemData configuredVerdantShard)
    {
        water = configuredWater;
        verdantShard = configuredVerdantShard;
        configured = true;
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression != null && progression.HasFlag(
            GameProgressionFlags.Chapter1VerdantShardObtained))
        {
            water?.SetActive(false);
            AbilityManager.Instance?.UnlockAbility(AbilityType.Dash);
        }
    }

    public void OnInteract()
    {
        if (!configured || transitioning || DialogueManager.Instance == null ||
            DialogueManager.Instance.IsDialogueActive)
            return;
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null || !progression.HasFlag(
            GameProgressionFlags.Chapter1ViridianIntroComplete) ||
            progression.HasFlag(GameProgressionFlags.Chapter1VerdantShardObtained))
            return;
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1RestrictedWheelInspected))
        {
            DialogueManager.Instance.StartDialogue(
                "chapter1.inspectRestrictedIrrigationWheel", () =>
                {
                    progression.SetFlag(
                        GameProgressionFlags.Chapter1RestrictedWheelInspected);
                    SetObjective("Interact with the irrigation wheel again.");
                });
            return;
        }
        RevealTruth(progression);
    }

    private void RevealTruth(GameProgressionManager progression)
    {
        transitioning = true;
        void ChangeSceneState()
        {
            water?.SetActive(false);
            void ContinueDialogue()
            {
                DialogueManager.Instance?.StartDialogue(
                    "chapter1.restrictedFarmlandsTruth",
                    () => CompleteObjective(progression));
            }
            if (ScreenFade.Instance != null)
                ScreenFade.Instance.FadeIn(ContinueDialogue);
            else
                ContinueDialogue();
        }
        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeOut(ChangeSceneState);
        else
            ChangeSceneState();
    }

    private void CompleteObjective(GameProgressionManager progression)
    {
        InventorySystem.Instance?.Add(verdantShard);
        AbilityManager.Instance?.UnlockAbility(AbilityType.Dash);
        progression.SetFlag(GameProgressionFlags.Chapter1VerdantShardObtained);
        SetObjective("The truth beneath the farmland has been revealed.");
        transitioning = false;
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}
