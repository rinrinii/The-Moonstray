using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    // Stores state per ID (NPCs, objects, etc.)
    private Dictionary<string, int> stateMap = new Dictionary<string, int>();

    // Stores global flags (quests, conditions, etc.)
    private Dictionary<string, bool> flagMap = new Dictionary<string, bool>();

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // STATE METHODS

    public int GetState(string id)
    {
        if (stateMap.ContainsKey(id))
            return stateMap[id];

        return 0; // default state
    }

    public void SetState(string id, int state)
    {
        stateMap[id] = state;
        Debug.Log($"[STATE] {id} -> {state}");
    }

    // FLAG METHODS

    public bool GetFlag(string key)
    {
        if (flagMap.ContainsKey(key))
            return flagMap[key];

        return false;
    }

    public void SetFlag(string key, bool value)
    {
        flagMap[key] = value;
        Debug.Log($"[FLAG] {key} -> {value}");
    }
}