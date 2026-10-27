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

    // =========================================
    // DEBUG
    // =========================================

    // Instantly unlock all player abilities.
    // This shortcut is intentionally available in
    // development builds for quickly testing areas
    // that require progression abilities.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            UnlockAbility(AbilityType.MoonWeave);
            Debug.Log("Moon Weave debug unlock enabled (F3).");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            UnlockAllAbilities();
        }
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

    public List<AbilityType> CaptureState()
    {
        List<AbilityType> abilities = new(unlockedAbilities);
        abilities.Sort();
        return abilities;
    }

    public void RestoreState(List<AbilityType> abilities)
    {
        unlockedAbilities.Clear();
        if (abilities != null)
        {
            foreach (AbilityType ability in abilities)
            {
                // Moon Weave was previously granted on entering Chapter 4,
                // so old saves can contain it before the Winter quest exists.
                // Ignore that legacy unlock; F3 is the temporary test route.
                if (ability != AbilityType.MoonWeave)
                    unlockedAbilities.Add(ability);
            }
        }
    }
}
