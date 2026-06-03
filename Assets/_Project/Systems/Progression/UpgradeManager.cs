using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Debug")]
    [SerializeField]
    private bool unlockAllUpgrades;

    private HashSet<UpgradeType>
        unlockedUpgrades =
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
        if (unlockAllUpgrades)
        {
            UnlockAllUpgrades();
        }
    }

    // use F10 to unlock all upgrades temporarily; for ease of debug and testing
    private void Update()
    {
#if UNITY_EDITOR

                if (Input.GetKeyDown(
                    KeyCode.F10))
                {
                    UnlockAllUpgrades();
                }

#endif
    }

    // =========================================
    // CHECKS
    // =========================================

    public bool IsUnlocked(
        UpgradeType upgrade)
    {
        return unlockedUpgrades.Contains(
            upgrade
        );
    }

    // =========================================
    // UNLOCK
    // =========================================

    public void UnlockUpgrade(
        UpgradeType upgrade)
    {
        if (unlockedUpgrades.Contains(
            upgrade))
        {
            return;
        }

        unlockedUpgrades.Add(
            upgrade
        );

        Debug.Log(
            $"Unlocked Upgrade: {upgrade}"
        );
    }

    // =========================================
    // LOCK
    // =========================================

    public void LockUpgrade(
        UpgradeType upgrade)
    {
        if (!unlockedUpgrades.Contains(
            upgrade))
        {
            return;
        }

        unlockedUpgrades.Remove(
            upgrade
        );

        Debug.Log(
            $"Locked Upgrade: {upgrade}"
        );
    }

    // =========================================
    // DEBUG
    // =========================================

    public void UnlockAllUpgrades()
    {
        foreach (UpgradeType upgrade
            in System.Enum.GetValues(
                typeof(UpgradeType)))
        {
            unlockedUpgrades.Add(
                upgrade
            );
        }

        Debug.Log(
            "All upgrades unlocked."
        );
    }

    public void LockAllUpgrades()
    {
        unlockedUpgrades.Clear();

        Debug.Log(
            "All upgrades locked."
        );
    }
}