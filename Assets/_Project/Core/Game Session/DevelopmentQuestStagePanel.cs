#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Development-only checkpoint picker for jumping into authored quest states.
/// Press F4 in play mode to open or close the panel.
/// </summary>
public sealed class DevelopmentQuestStagePanel : MonoBehaviour
{
    private sealed class Checkpoint
    {
        public readonly string Name;
        public readonly string Description;
        public readonly GameProgressionStage Stage;
        public readonly string Scene;
        public readonly string SpawnID;
        public readonly string[] Flags;
        public readonly bool UnlockDash;
        public readonly TutorialState? TutorialState;

        public Checkpoint(
            string name,
            string description,
            GameProgressionStage stage,
            string scene,
            string spawnID,
            bool unlockDash,
            params string[] flags)
        {
            Name = name;
            Description = description;
            Stage = stage;
            Scene = scene;
            SpawnID = spawnID;
            UnlockDash = unlockDash;
            Flags = flags ?? Array.Empty<string>();
            TutorialState = null;
        }

        public Checkpoint(
            string name,
            string description,
            TutorialState tutorialState,
            string scene,
            string spawnID)
        {
            Name = name;
            Description = description;
            Stage = GameProgressionStage.Tutorial;
            Scene = scene;
            SpawnID = spawnID;
            UnlockDash = false;
            Flags = Array.Empty<string>();
            TutorialState = tutorialState;
        }
    }

    private static readonly string[] ThroughGuide =
    {
        GameProgressionFlags.Chapter1GuideIntroComplete
    };

    private static readonly string[] ThroughBloombridge = Add(
        ThroughGuide,
        GameProgressionFlags.Chapter1ArrivedAtBloombridge);

    private static readonly string[] ThroughFarmer = Add(
        ThroughBloombridge,
        GameProgressionFlags.Chapter1FarmerIntroComplete);

    private static readonly string[] ThroughWitheredCrops = Add(
        ThroughFarmer,
        GameProgressionFlags.Chapter1WitheredCropsInspected);

    private static readonly string[] ThroughIrrigation = Add(
        ThroughWitheredCrops,
        GameProgressionFlags.Chapter1IrrigationInspected);

    private static readonly string[] ThroughGreenhouse = Add(
        ThroughIrrigation,
        GameProgressionFlags.Chapter1GreenhouseInspected);

    private static readonly string[] ThroughVillageInspection = Add(
        ThroughGreenhouse,
        GameProgressionFlags.Chapter1VillageIrrigationInspected,
        GameProgressionFlags.Chapter1ShovelObtained);

    private static readonly string[] ThroughVillageRestoration = Add(
        ThroughVillageInspection,
        GameProgressionFlags.Chapter1VillageIrrigationRestored);

    private static readonly string[] ThroughFirstOvergrowthCrop = Add(
        ThroughVillageRestoration,
        GameProgressionFlags.Chapter1OvergrowthCropOneInspected);

    private static readonly string[] ThroughOvergrowth = Add(
        ThroughFirstOvergrowthCrop,
        GameProgressionFlags.Chapter1OvergrowthCropTwoInspected);

    private static readonly string[] ThroughViridian = Add(
        ThroughOvergrowth,
        GameProgressionFlags.Chapter1ViridianIntroComplete);

    private static readonly string[] ThroughRestrictedNote = Add(
        ThroughViridian,
        GameProgressionFlags.Chapter1RestrictedWeatheredNoteRead,
        GameProgressionFlags.Chapter1RestrictedWheelInspected);

    private static readonly string[] SpringComplete = Add(
        ThroughRestrictedNote,
        GameProgressionFlags.Chapter1RestrictedWheelCropsActivated,
        GameProgressionFlags.Chapter1RestrictedWheelSilosActivated,
        GameProgressionFlags.Chapter1VerdantShardObtained,
        GameProgressionFlags.Chapter1ReturnToMoonveil);

    private static readonly Checkpoint[] Checkpoints =
    {
        new(
            "Tutorial — wake in Frostmere Library",
            "Starts at the library wake-up and student dialogue sequence.",
            TutorialState.WakeInLibrary,
            "Frostmere Library",
            "ToRestrictedArchivesFromEastWing"),
        new(
            "Spring — talk to the guide",
            "Starts Chapter 1 in Moonveil before the guide introduction.",
            GameProgressionStage.Chapter1Spring,
            "Moonveil",
            "CenterMoonveil",
            false),
        new(
            "Spring — arrive at Bloombridge",
            "Skips the tutorial and guide, then starts the journey into Springtide.",
            GameProgressionStage.Chapter1Spring,
            "Bloombridge Path",
            "ToBloombridgePath",
            false,
            ThroughGuide),
        new(
            "Spring — talk to the farmer",
            "Begins the Outer Farmlands investigation.",
            GameProgressionStage.Chapter1Spring,
            "Outer Farmlands",
            "ToFarmlandsFromBloombridge",
            false,
            ThroughBloombridge),
        new(
            "Spring — inspect irrigation",
            "Places the investigation after the withered crops.",
            GameProgressionStage.Chapter1Spring,
            "Outer Farmlands",
            "ToFarmlandsFromBloombridge",
            false,
            ThroughWitheredCrops),
        new(
            "Spring — inspect greenhouse",
            "Places the investigation after the damaged irrigation.",
            GameProgressionStage.Chapter1Spring,
            "Outer Farmlands",
            "ToFarmlandsFromBloombridge",
            false,
            ThroughIrrigation),
        new(
            "Spring — enter Village Basin",
            "Starts the old-irrigation objective after the farmland investigation.",
            GameProgressionStage.Chapter1Spring,
            "Village Basin",
            "ToVillageBasinFromFarmlands",
            false,
            ThroughGreenhouse),
        new(
            "Spring — restore village irrigation",
            "Skips its inspection and grants the shovel progression flag.",
            GameProgressionStage.Chapter1Spring,
            "Village Basin",
            "ToVillageBasinFromFarmlands",
            false,
            ThroughVillageInspection),
        new(
            "Spring — investigate Overgrowth Fields",
            "Starts the first rotting-crop investigation.",
            GameProgressionStage.Chapter1Spring,
            "Overgrowth Fields",
            "ToOvergrowthFieldsFromVillageBasin",
            false,
            ThroughVillageRestoration),
        new(
            "Spring — inspect second overgrowth crop",
            "Places the field investigation between its two crop clues.",
            GameProgressionStage.Chapter1Spring,
            "Overgrowth Fields",
            "ToOvergrowthFieldsFromVillageBasin",
            false,
            ThroughFirstOvergrowthCrop),
        new(
            "Spring — talk to Viridian",
            "Finishes the field clues and moves to Viridian Estate.",
            GameProgressionStage.Chapter1Spring,
            "Viridian Estate",
            "ToViridianEstateFromBasin",
            false,
            ThroughOvergrowth),
        new(
            "Spring — enter Restricted Farmlands",
            "Skips Viridian's introduction and starts the final investigation.",
            GameProgressionStage.Chapter1Spring,
            "Restricted Farmlands",
            "ToRestrictedFarmlands",
            false,
            ThroughViridian),
        new(
            "Spring — solve the wheel puzzle",
            "Starts immediately after reading the weathered note.",
            GameProgressionStage.Chapter1Spring,
            "Restricted Farmlands",
            "ToRestrictedFarmlands",
            false,
            ThroughRestrictedNote),
        new(
            "Summer — chapter start",
            "Treats Spring as complete and enters the Industrial Frontier.",
            GameProgressionStage.Chapter2Summer,
            "Industrial Frontier",
            "ToIndustrialFrontierFromMoonveil",
            true,
            SpringComplete),
        new(
            "Autumn — chapter start",
            "Sets the Autumn stage and enters Amber Path.",
            GameProgressionStage.Chapter3Autumn,
            "Amber Path",
            "ToAmberPathFromMoonveil",
            true,
            SpringComplete)
    };

    private static DevelopmentQuestStagePanel instance;

    private bool isOpen;
    private Vector2 scrollPosition;
    private Rect windowRect;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private GUIStyle titleStyle;
    private GUIStyle descriptionStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        GameObject host = new(nameof(DevelopmentQuestStagePanel));
        instance = host.AddComponent<DevelopmentQuestStagePanel>();
        DontDestroyOnLoad(host);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        windowRect = new Rect(0f, 0f, 720f, 680f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
            SetOpen(!isOpen);
        else if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            SetOpen(false);
    }

    private void OnGUI()
    {
        if (!isOpen)
            return;

        float width = Mathf.Min(720f, Screen.width - 32f);
        float height = Mathf.Min(680f, Screen.height - 32f);
        windowRect.width = width;
        windowRect.height = height;

        if (windowRect.x == 0f && windowRect.y == 0f)
        {
            windowRect.x = (Screen.width - width) * 0.5f;
            windowRect.y = (Screen.height - height) * 0.5f;
        }

        windowRect = GUI.Window(
            GetInstanceID(),
            windowRect,
            DrawWindow,
            "Quest Stage Jumper — F4 to close");
    }

    private void DrawWindow(int windowID)
    {
        EnsureStyles();

        GUILayout.Space(8f);
        GUILayout.Label(
            "Development only. Choosing a checkpoint replaces current quest " +
            "and progression state; it does not write a save file.",
            descriptionStyle);
        GUILayout.Space(8f);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (Checkpoint checkpoint in Checkpoints)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(checkpoint.Name, titleStyle);
            GUILayout.Label(checkpoint.Description, descriptionStyle);

            if (GUILayout.Button($"Jump to {checkpoint.Scene}", GUILayout.Height(32f)))
            {
                ApplyCheckpoint(checkpoint);
                GUIUtility.ExitGUI();
            }

            GUILayout.EndVertical();
            GUILayout.Space(4f);
        }

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 28f));
    }

    private void ApplyCheckpoint(Checkpoint checkpoint)
    {
        GameProgressionManager progression = GameProgressionManager.Instance;
        if (progression == null)
        {
            Debug.LogError("Quest Stage Jumper could not find GameProgressionManager.");
            return;
        }

        QuestManager.Instance?.ResetProgress();

        if (checkpoint.TutorialState.HasValue)
            TutorialManager.Instance?.RestoreState(checkpoint.TutorialState.Value);
        else
            TutorialManager.Instance?.FinishTutorial();

        progression.RestoreState(new GameProgressionManager.Snapshot
        {
            stage = checkpoint.Stage,
            flags = new List<string>(checkpoint.Flags)
        });

        AbilityManager.Instance?.LockAllAbilities();
        AbilityManager.Instance?.UnlockAbility(AbilityType.Climb);
        if (checkpoint.UnlockDash)
            AbilityManager.Instance?.UnlockAbility(AbilityType.Dash);

        InventoryUI.Instance?.RestoreUnlockState(true);
        GameplayUIManager.Instance?.Map?.RestoreUnlockState(true);
        PlayerTransformation.Instance?.UnlockTransformation();

        SetOpen(false);
        Debug.Log($"Quest Stage Jumper: loading '{checkpoint.Name}'.");
        SceneLoader.LoadScene(checkpoint.Scene, checkpoint.SpawnID);
    }

    private void SetOpen(bool open)
    {
        if (isOpen == open)
            return;

        isOpen = open;
        if (open)
        {
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            windowRect.x = 0f;
            windowRect.y = 0f;
        }
        else
        {
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        descriptionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            wordWrap = true
        };
    }

    private static string[] Add(string[] existing, params string[] additions)
    {
        string[] result = new string[existing.Length + additions.Length];
        Array.Copy(existing, result, existing.Length);
        Array.Copy(additions, 0, result, existing.Length, additions.Length);
        return result;
    }
}
#endif
