using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerWeave : MonoBehaviour
{
    private const string ArrivalSpawnID = "LunariaTeleportArrival";

    private sealed class Destination
    {
        public readonly string Region;
        public readonly string Scene;
        public readonly string SpawnID;

        public Destination(string region, string scene, string spawnID)
        {
            Region = region;
            Scene = scene;
            SpawnID = spawnID;
        }
    }

    private static readonly Destination[] Destinations =
    {
        new("Moonveil", "Moonveil", "CenterMoonveil"),
        new("Springtide Meadows", "Bloom Cavern", ArrivalSpawnID),
        new("Mirage Plains", "Industrial Frontier", ArrivalSpawnID),
        new("Amberwood Forest", "Trade District", ArrivalSpawnID),
        new("Pale Snowfields", "Frostmere Library", ArrivalSpawnID)
    };

    private static bool arrivalFadePending;
    private static bool isTransitioning;

    public static void CompleteArrivalTransition()
    {
        if (!isTransitioning)
            return;

        isTransitioning = false;

        if (!arrivalFadePending)
            return;

        arrivalFadePending = false;

        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeIn();
    }

    public void OpenDestinationPrompt()
    {
        if (isTransitioning)
            return;

        AbilityManager abilities = AbilityManager.Instance;
        if (abilities == null || !abilities.IsUnlocked(AbilityType.MoonWeave))
            return;

        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null || dialogue.IsDialogueActive)
            return;

        List<DialogueChoice> choices = new();
        foreach (Destination destination in Destinations)
        {
            Destination selectedDestination = destination;
            choices.Add(new DialogueChoice(
                selectedDestination.Region,
                () => TeleportTo(selectedDestination)));
        }

        choices.Add(new DialogueChoice("Cancel", null));
        dialogue.ShowPrompt("Where would you like to teleport?", choices);
    }

    private void TeleportTo(Destination destination)
    {
        if (isTransitioning || destination == null)
            return;

        isTransitioning = true;

        void LoadDestination()
        {
            SceneLoader.LoadScene(destination.Scene, destination.SpawnID);
        }

        if (ScreenFade.Instance != null)
        {
            arrivalFadePending = true;
            ScreenFade.Instance.FadeOut(LoadDestination);
        }
        else
            LoadDestination();
    }
}
