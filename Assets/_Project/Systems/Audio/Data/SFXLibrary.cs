using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SFX Library",
    menuName = "_Project/Systems/Audio/Data/SFX Library")]
public class SFXLibrary : ScriptableObject
{
    [SerializeField]
    private List<SFXLibraryEntry> entries = new();

    public SFXSet GetSFXSet(string id)
    {
        foreach (var entry in entries)
        {
            if (entry.id == id)
                return entry.sfxSet;
        }

        Debug.LogWarning($"SFX ID '{id}' was not found in the SFX Library.");
        return null;
    }
}