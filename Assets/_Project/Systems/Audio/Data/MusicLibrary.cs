using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Music Library",
    menuName = "Moonstray/Audio/Music Library")]
public class MusicLibrary : ScriptableObject
{
    [SerializeField]
    private List<MusicLibraryEntry> entries = new();

    public MusicSet GetMusicSet(string id)
    {
        foreach (var entry in entries)
        {
            if (entry.id == id)
                return entry.musicSet;
        }

        Debug.LogWarning($"Music ID '{id}' was not found in the Music Library.");
        return null;
    }
}