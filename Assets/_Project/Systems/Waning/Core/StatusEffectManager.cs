using UnityEngine;
using System;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    public event Action OnStatusChanged;

    private int slowSources = 0;
    public bool HasSlow => slowSources > 0;

    public bool HasThorn { get; private set; }
    public bool HasFrostbite { get; private set; }
    public bool HasPoison { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddSlow()
    {
        slowSources++;
        OnStatusChanged?.Invoke();
    }

    public void RemoveSlow()
    {
        slowSources = Mathf.Max(0, slowSources - 1);
        OnStatusChanged?.Invoke();
    }

    public void SetThorn(bool active)
    {
        HasThorn = active;
        OnStatusChanged?.Invoke();
    }

    public void SetFrostbite(bool active)
    {
        HasFrostbite = active;
        OnStatusChanged?.Invoke();
    }

    public void SetPoison(bool active)
    {
        HasPoison = active;
        OnStatusChanged?.Invoke();
    }

    public void ClearAll()
    {
        slowSources = 0;

        HasThorn = false;
        HasFrostbite = false;
        HasPoison = false;

        PlayerTransformation.Instance?.SetSpeedModifier(1f);

        OnStatusChanged?.Invoke();
    }
}