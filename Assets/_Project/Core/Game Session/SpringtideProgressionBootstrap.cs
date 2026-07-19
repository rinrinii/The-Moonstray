using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpringtideProgressionBootstrap : MonoBehaviour
{
    private const string QuestTitle = "For Every Garden Buries a Secret";
    private const string QuestID =
        "chapter1.for_every_garden_buries_a_secret";
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
            }

            // Always restore the objective when Bloombridge loads. The arrival
            // flag may already be present in a save even though the UI/journal
            // objective was never recorded (or was cleared during scene load).
            ObjectivesUI.Instance?.SetObjective(
                "chapter1.new_beginnings",
                "visit_outer_farmlands",
                0);
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
            "inspect_damaged_irrigation");
        ConfigureInspection(
            "inspectDestroyedIrrigation1",
            "chapter1.inspectDamagedIrrigation",
            GameProgressionFlags.Chapter1WitheredCropsInspected,
            GameProgressionFlags.Chapter1IrrigationInspected,
            "inspect_abandoned_greenhouse");
        ConfigureInspection(
            "inspectAbandonedGreenhouse1",
            "chapter1.inspectAbandonedGreenhouse",
            GameProgressionFlags.Chapter1IrrigationInspected,
            GameProgressionFlags.Chapter1GreenhouseInspected,
            "visit_village_basin");

        RestoreCurrentObjective(progression);

        GameObject greenhouse = GameObject.Find("greenhouse-ruins (1)");
        if (greenhouse != null)
            ConfigureAreaOfInterestMarker(greenhouse, "repairGreenhouse");
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

        ConfigureQuestMarker(viridian, "viridianNPC");

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
                    objectiveQuestIDOnComplete = QuestID,
                    objectiveIDOnComplete = "visit_restricted_farmlands"
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
            SetAssetObjective("visit_restricted_farmlands");
        else
            SetAssetObjective("talk_to_viridian");
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

        GameObject water = GameObject.Find("Water");
        GameObject weatheredNote = GameObject.Find("weatheredNote");
        ItemData verdantShard =
            Resources.Load<ItemData>("Items/Fragments/Verdant Shard");

        if (weatheredNote != null)
        {
            ConfigureQuestMarker(weatheredNote, "weatheredNote");
            NoteInteractionResponse note =
                weatheredNote.GetComponent<NoteInteractionResponse>();
            note?.ConfigureOnRead(() =>
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1RestrictedWeatheredNoteRead);
                progression.SetFlag(
                    GameProgressionFlags.Chapter1RestrictedWheelInspected);
                SetAssetObjective("solve_irrigation_wheels");
                MapMarkerController.Instance?.RefreshMarkers();
            });
        }

        foreach (ObjectStateInteraction stateInteraction in interactions)
        {
            if (stateInteraction.ObjectID != "irrigationWheel")
                continue;

            ConfigureQuestMarker(stateInteraction.gameObject, "irrigationWheel");

            RestrictedFarmlandsQuestInteraction quest =
                stateInteraction.GetComponent<
                    RestrictedFarmlandsQuestInteraction>();
            if (quest == null)
            {
                quest = stateInteraction.gameObject.AddComponent<
                    RestrictedFarmlandsQuestInteraction>();
            }

            quest.Configure(
                water,
                verdantShard);
        }

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1VerdantShardObtained))
        {
            waterStateObjectiveComplete();
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1RestrictedWeatheredNoteRead))
        {
            RestoreRestrictedFarmlandsWheelObjective(progression);
        }
        else
        {
            SetAssetObjective("inspect_irrigation_wheel");
        }

        MapMarkerController.Instance?.RefreshMarkers();

        static void waterStateObjectiveComplete()
        {
            SetObjective("The truth beneath the farmland has been revealed.");
        }
    }

    private static void RestoreRestrictedFarmlandsWheelObjective(
        GameProgressionManager progression)
    {
        if (progression.HasFlag(
            GameProgressionFlags.Chapter1RestrictedWheelSilosActivated))
        {
            SetRestrictedWheelObjective(2);
            return;
        }

        if (progression.HasFlag(
            GameProgressionFlags.Chapter1RestrictedWheelCropsActivated))
        {
            SetRestrictedWheelObjective(1);
            return;
        }

        SetRestrictedWheelObjective(0);
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
                ConfigureQuestMarker(stateInteraction.gameObject, "inspectRottenCrop2");
                ConfigureOvergrowthInteraction(
                    stateInteraction.gameObject,
                    OvergrowthFieldsQuestInteraction.Step.CropOne);
            }
            else if (stateInteraction.ObjectID == "inspectRottenCrop3")
            {
                ConfigureQuestMarker(stateInteraction.gameObject, "inspectRottenCrop3");
                ConfigureOvergrowthInteraction(
                    stateInteraction.gameObject,
                    OvergrowthFieldsQuestInteraction.Step.CropTwo);
            }
        }

        if (progression != null)
            RestoreOvergrowthObjective(progression);

        GameObject ruinedGarden = GameObject.Find("ruined-garden 1");
        if (ruinedGarden != null)
            ConfigureAreaOfInterestMarker(ruinedGarden, "repairRuinedGarden");
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
            SetAssetObjective("find_harvest_steward");
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
        {
            ObjectivesUI.Instance?.SetObjective(
                QuestID, "inspect_second_rotting_crop", 0);
        }
        else
        {
            ObjectivesUI.Instance?.SetObjective(
                QuestID, "inspect_rotting_crops", 0);
        }

        MapMarkerController.Instance?.RefreshMarkers();
    }

    private static void ConfigureVillageBasin(
        GameProgressionManager progression)
    {
        if (!progression.HasFlag(
            GameProgressionFlags.Chapter1GreenhouseInspected))
        {
            return;
        }

        // Revisiting Village Basin must not rewind the chapter to its local
        // irrigation step after the player has progressed into Overgrowth.
        if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropTwoInspected))
        {
            SetAssetObjective("find_harvest_steward");
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
        {
            SetAssetObjective("inspect_second_rotting_crop");
        }
        else if (progression.HasFlag(
            GameProgressionFlags.Chapter1VillageIrrigationRestored))
        {
            SetAssetObjective("investigate_waning");
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
            ConfigureQuestMarker(
                stateInteraction.gameObject,
                "restoreDestroyedIrrigation2");
            ConfigureAreaOfInterestMarker(
                stateInteraction.gameObject,
                "repairIrrigationSystem");

            RestorationPuzzleInteraction puzzle =
                stateInteraction.GetComponent<RestorationPuzzleInteraction>();
            if (puzzle == null)
            {
                puzzle = stateInteraction.gameObject.AddComponent<
                    RestorationPuzzleInteraction>();
            }

            puzzle.ConfigureSingleMaterial(
                restore,
                Resources.Load<Texture2D>("Puzzles/irrigationRestored"),
                Resources.Load<ItemData>("Items/Shovel"),
                1,
                "chapter1.inspectVillageIrrigation",
                "",
                "chapter1.restoreVillageIrrigation",
                GameProgressionFlags.Chapter1VillageIrrigationInspected,
                GameProgressionFlags.Chapter1VillageIrrigationRestored,
                QuestID,
                "obtain_shovel",
                "restore_irrigation",
                "investigate_waning",
                "Restore the Irrigation System",
                "Rotate each fragment to reconstruct the irrigation system.");

            if (progression.HasFlag(
                GameProgressionFlags.Chapter1OvergrowthCropTwoInspected))
            {
                SetAssetObjective("find_harvest_steward");
            }
            else if (progression.HasFlag(
                GameProgressionFlags.Chapter1OvergrowthCropOneInspected))
            {
                SetAssetObjective("inspect_second_rotting_crop");
            }
            else if (progression.HasFlag(
                GameProgressionFlags.Chapter1VillageIrrigationRestored))
            {
                SetAssetObjective("investigate_waning");
            }
            else if (progression.HasFlag(
                GameProgressionFlags.Chapter1VillageIrrigationInspected))
            {
                if (InventoryContains(Resources.Load<ItemData>("Items/Shovel")))
                {
                    progression.SetFlag(
                        GameProgressionFlags.Chapter1ShovelObtained);
                    SetAssetObjective("restore_irrigation");
                }
                else
                {
                    SetAssetObjective("obtain_shovel");
                }
            }
            else
            {
                SetAssetObjective("inspect_old_irrigation");
            }

            MapMarkerController.Instance?.RefreshMarkers();
            break;
        }

        GameObject ruinedSilo = GameObject.Find("silo-ruined");
        if (ruinedSilo != null)
            ConfigureAreaOfInterestMarker(ruinedSilo, "repairSilo");

        GameObject shovel = GameObject.Find("shovel");
        if (shovel != null)
            ConfigureQuestMarker(shovel, "villageBasinShovel");
    }

    private static bool InventoryContains(ItemData item)
    {
        if (item == null || InventorySystem.Instance == null)
            return false;

        foreach (InventorySystem.Slot slot in InventorySystem.Instance.slots)
        {
            if (slot.amount > 0 && slot.item != null &&
                (slot.item == item || slot.item.itemID == item.itemID))
            {
                return true;
            }
        }

        return false;
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

        ConfigureQuestMarker(farmer, "farmerNPC");

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
                    objectiveQuestIDOnComplete = QuestID,
                    objectiveIDOnComplete = "inspect_withered_crops"
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

            ConfigureQuestMarker(stateInteraction.gameObject, objectID);

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
            SetAssetObjective("visit_village_basin");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1IrrigationInspected))
            SetAssetObjective("inspect_abandoned_greenhouse");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1WitheredCropsInspected))
            SetAssetObjective("inspect_damaged_irrigation");
        else if (progression.HasFlag(GameProgressionFlags.Chapter1FarmerIntroComplete))
            SetAssetObjective("inspect_withered_crops");
        else
            SetAssetObjective("talk_to_farmer");

        MapMarkerController.Instance?.RefreshMarkers();
    }

    private static void ConfigureQuestMarker(GameObject target, string markerID)
    {
        MapMarkerTarget marker = target.GetComponent<MapMarkerTarget>();
        if (marker == null)
            marker = target.AddComponent<MapMarkerTarget>();

        marker.Configure(markerID, MapMarkerType.Quest);
    }

    private static void ConfigureAreaOfInterestMarker(
        GameObject target,
        string markerID)
    {
        MapMarkerTarget marker = null;
        foreach (MapMarkerTarget candidate in
            target.GetComponents<MapMarkerTarget>())
        {
            if (candidate.MarkerID == markerID)
            {
                marker = candidate;
                break;
            }
        }

        if (marker == null)
            marker = target.AddComponent<MapMarkerTarget>();

        marker.Configure(markerID, MapMarkerType.POI);
        MapMarkerController.Instance?.RefreshMarkers();
    }

    private static void SetAssetObjective(string objectiveID)
    {
        ObjectivesUI.Instance?.SetObjective(QuestID, objectiveID, 0);
    }

    private static void SetRestrictedWheelObjective(int currentAmount)
    {
        ObjectivesUI.Instance?.SetObjective(
            QuestID,
            "solve_irrigation_wheels",
            currentAmount);
    }

    private static void SetObjective(string description)
    {
        ObjectivesUI.Instance?.SetObjective(QuestTitle, description);
    }
}

public class RestrictedFarmlandsQuestInteraction : MonoBehaviour,
    IInteractionResponse
{
    private const string QuestID =
        "chapter1.for_every_garden_buries_a_secret";
    private const string QuestTitle = "For Every Garden Buries a Secret";
    private const string CropsWheelName = "irrigationWheelCrops";
    private const string SilosWheelName = "irrigationWheelSilos";
    private const string PondWheelName = "irrigationWheelPond";
    private const string DirtTerrainLayerName = "Dirt_2_Dark";
    private const string SpringWaningTerrainLayerPath =
        "TerrainLayers/springWaningTexture";
    private GameObject water;
    private ItemData verdantShard;
    private string wheelName;
    private bool configured;
    private bool transitioning;

    public void Configure(
        GameObject configuredWater,
        ItemData configuredVerdantShard)
    {
        water = configuredWater;
        verdantShard = configuredVerdantShard;
        wheelName = gameObject.name;
        configured = true;
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression != null && progression.HasFlag(
            GameProgressionFlags.Chapter1VerdantShardObtained))
        {
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
            water?.SetActive(false);
            ReplaceDarkDirtTerrainLayer();
            AbilityManager.Instance?.UnlockAbility(AbilityType.Dash);
        }
        else if (progression != null &&
            ((wheelName == CropsWheelName && progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelCropsActivated)) ||
             (wheelName == SilosWheelName && progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelSilosActivated))))
        {
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
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
            GameProgressionFlags.Chapter1RestrictedWeatheredNoteRead))
            return;

        HandleWheelSequence(progression);
    }

    private void HandleWheelSequence(GameProgressionManager progression)
    {
        if (wheelName == CropsWheelName)
        {
            if (!progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelCropsActivated))
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1RestrictedWheelCropsActivated);
            }

            GetComponent<ObjectStateHighlightMarker>()?.Hide();
            SetRestrictedWheelObjective(1);
            return;
        }

        if (wheelName == SilosWheelName)
        {
            if (!progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelCropsActivated))
            {
                ResetWheelSequence(progression);
                return;
            }

            if (!progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelSilosActivated))
            {
                progression.SetFlag(
                    GameProgressionFlags.Chapter1RestrictedWheelSilosActivated);
            }

            GetComponent<ObjectStateHighlightMarker>()?.Hide();
            SetRestrictedWheelObjective(2);
            return;
        }

        if (wheelName == PondWheelName &&
            progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelCropsActivated) &&
            progression.HasFlag(
                GameProgressionFlags.Chapter1RestrictedWheelSilosActivated))
        {
            GetComponent<ObjectStateHighlightMarker>()?.Hide();
            RevealTruth(progression);
            return;
        }

        ResetWheelSequence(progression);
    }

    private void ResetWheelSequence(GameProgressionManager progression)
    {
        progression.SetFlag(
            GameProgressionFlags.Chapter1RestrictedWheelCropsActivated,
            false);
        progression.SetFlag(
            GameProgressionFlags.Chapter1RestrictedWheelSilosActivated,
            false);
        SetRestrictedWheelObjective(0);
    }

    private void RevealTruth(GameProgressionManager progression)
    {
        transitioning = true;
        void ChangeSceneState()
        {
            water?.SetActive(false);
            ReplaceDarkDirtTerrainLayer();
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

    private static void SetRestrictedWheelObjective(int currentAmount)
    {
        ObjectivesUI.Instance?.SetObjective(
            QuestID,
            "solve_irrigation_wheels",
            currentAmount);
    }

    private static void ReplaceDarkDirtTerrainLayer()
    {
        TerrainLayer springWaningLayer =
            Resources.Load<TerrainLayer>(SpringWaningTerrainLayerPath);
        if (springWaningLayer == null)
        {
            Debug.LogWarning(
                $"Missing terrain layer resource: {SpringWaningTerrainLayerPath}");
            return;
        }

        Terrain[] terrains = Object.FindObjectsByType<Terrain>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null)
                continue;

            TerrainLayer[] layers = terrainData.terrainLayers;
            bool replaced = false;

            for (int i = 0; i < layers.Length; i++)
            {
                TerrainLayer layer = layers[i];
                if (layer == null || layer.name != DirtTerrainLayerName)
                    continue;

                layers[i] = springWaningLayer;
                replaced = true;
            }

            if (replaced)
                terrainData.terrainLayers = layers;
        }
    }
}
