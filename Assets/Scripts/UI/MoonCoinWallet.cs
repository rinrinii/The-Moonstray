using System;
using UnityEngine;

public class MoonCoinWallet : MonoBehaviour
{
    public const int StartingMoonCoins = 250;
    public static MoonCoinWallet Instance { get; private set; }

    public event Action<int> OnMoonCoinsChanged;

    [SerializeField]
    private int moonCoins = StartingMoonCoins;

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

    public void SetAmount(int amount)
    {
        moonCoins = Mathf.Max(0, amount);
        OnMoonCoinsChanged?.Invoke(moonCoins);
    }

    public void ResetState() => SetAmount(StartingMoonCoins);
}
