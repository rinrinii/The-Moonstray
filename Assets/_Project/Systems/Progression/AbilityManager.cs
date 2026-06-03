using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("Debug")]
    [SerializeField]
    private bool unlockAllAbilities;

    private HashSet<AbilityType>
        unlockedAbilities =
            new();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (unlockAllAbilities)
        {
            UnlockAllAbilities();
            return;
        }

        UnlockAbility(
            AbilityType.Climb
        );
    }

    // use F9 to unlock all abilities temporarily; for ease of debug and testing
    private void Update()
    {
        #if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.F9))
        {
            UnlockAllAbilities();
        }

        #endif
    }

    // =========================================
    // CHECKS
    // =========================================

    public bool IsUnlocked(
        AbilityType ability)
    {
        return unlockedAbilities.Contains(
            ability
        );
    }

    // =========================================
    // UNLOCK
    // =========================================

    public void UnlockAbility(
        AbilityType ability)
    {
        if (unlockedAbilities.Contains(
            ability))
        {
            return;
        }

        unlockedAbilities.Add(
            ability
        );

        Debug.Log(
            $"Unlocked Ability: {ability}"
        );
    }

    // =========================================
    // DEBUG
    // =========================================

    public void UnlockAllAbilities()
    {
        foreach (AbilityType ability
            in System.Enum.GetValues(
                typeof(AbilityType)))
        {
            unlockedAbilities.Add(
                ability
            );
        }

        Debug.Log(
            "All abilities unlocked."
        );
    }

    public void LockAllAbilities()
    {
        unlockedAbilities.Clear();

        Debug.Log(
            "All abilities locked."
        );
    }
}