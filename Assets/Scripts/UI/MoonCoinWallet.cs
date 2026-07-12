using System;
using UnityEngine;

public class MoonCoinWallet : MonoBehaviour
{
    public static MoonCoinWallet Instance { get; private set; }

    public event Action<int> OnMoonCoinsChanged;

    [SerializeField]
    private int moonCoins = 250;

    public int MoonCoins => moonCoins;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
            return;

        if (FindFirstObjectByType<MoonCoinWallet>(
                FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject walletObject = new GameObject(nameof(MoonCoinWallet));
        walletObject.AddComponent<MoonCoinWallet>();
        DontDestroyOnLoad(walletObject);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GetComponentInParent<PersistentRoot>() == null)
            DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount)
    {
        if (amount <= 0)
            return;

        moonCoins += amount;
        OnMoonCoinsChanged?.Invoke(moonCoins);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0)
            return true;

        if (moonCoins < amount)
            return false;

        moonCoins -= amount;
        OnMoonCoinsChanged?.Invoke(moonCoins);
        return true;
    }
}
