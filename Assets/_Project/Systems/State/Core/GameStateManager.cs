using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    // Fired whenever an object's state changes.
    public event Action<string, int> OnObjectStateChanged;

    // Stores state per ID (NPCs, objects, etc.)
    private readonly Dictionary<string, int> stateMap = new();

    // Stores global flags (quests, conditions, etc.)
    private readonly Dictionary<string, bool> flagMap = new();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ============================
    // STATE METHODS
    // ============================

    public int GetState(string id)
    {
        if (stateMap.TryGetValue(id, out int state))
            return state;

        return 0; // Default state
    }

    public void SetState(string id, int state)
    {
        stateMap[id] = state;

        Debug.Log($"[STATE] {id} -> {state}");

        OnObjectStateChanged?.Invoke(id, state);
    }

    // ============================
    // FLAG METHODS
    // ============================

    public bool GetFlag(string key)
    {
        if (flagMap.TryGetValue(key, out bool value))
            return value;

        return false;
    }

    public void SetFlag(string key, bool value)
    {
        flagMap[key] = value;

        Debug.Log($"[FLAG] {key} -> {value}");
    }
}