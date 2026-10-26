using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerAbilityWheel : MonoBehaviour
{
    private enum RAbility
    {
        Sense,
        Howl,
        Teleport
    }

    private const float HoldDuration = 0.25f;
    private const float SelectionDeadZone = 70f;
    private const float DirectionThreshold = 0.35f;

    private static RAbility selectedAbility = RAbility.Sense;

    private PlayerLunarSense lunarSense;
    private PlayerHowl howl;
    private PlayerWeave weave;
    private AbilityRadialWheelView view;

    private float pressStartedAt;
    private bool isPressed;
    private bool isWheelOpen;
    private RAbility? highlightedAbility;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    public bool IsInputCaptured => isPressed || isWheelOpen;

    private void Awake()
    {
        lunarSense = GetComponent<PlayerLunarSense>();
        howl = GetComponent<PlayerHowl>();
        weave = GetComponent<PlayerWeave>();
        if (weave == null)
            weave = gameObject.AddComponent<PlayerWeave>();
    }

    private void Update()
    {
        EnsureView();
        RefreshAvailability();

        if (!CanUseAbilityInput())
        {
            CancelInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isPressed = true;
            pressStartedAt = Time.unscaledTime;
        }

        if (isPressed && !isWheelOpen && Input.GetKey(KeyCode.R) &&
            Time.unscaledTime - pressStartedAt >= HoldDuration)
        {
            OpenWheel();
        }

        if (isWheelOpen)
            UpdateWheelSelection();

        if (isPressed && Input.GetKeyUp(KeyCode.R))
        {
            if (isWheelOpen)
                CommitWheelSelection();
            else
                ActivateSelectedAbility();

            isPressed = false;
        }
    }

    private void EnsureView()
    {
        if (view == null)
            view = AbilityRadialWheelView.Create(transform);
    }

    private void RefreshAvailability()
    {
        if (view == null)
            return;

        bool senseAvailable = IsAvailable(RAbility.Sense);
        bool howlAvailable = IsAvailable(RAbility.Howl);
        bool teleportAvailable = IsAvailable(RAbility.Teleport);

        if (!IsAvailable(selectedAbility))
        {
            if (senseAvailable)
                selectedAbility = RAbility.Sense;
            else if (howlAvailable)
                selectedAbility = RAbility.Howl;
            else if (teleportAvailable)
                selectedAbility = RAbility.Teleport;
        }

        view.SetAbilityAvailable("Sense", senseAvailable);
        view.SetAbilityAvailable("Howl", howlAvailable);
        view.SetAbilityAvailable("Teleport", teleportAvailable);

        bool showHud = (senseAvailable || howlAvailable || teleportAvailable) &&
            !GameplayUIManager.IsCaptureHudHidden;
        view.SetHudVisible(showHud);
        view.SetEquippedAbility(
            IsAvailable(selectedAbility)
                ? GetDisplayName(selectedAbility)
                : string.Empty);
    }

    private bool CanUseAbilityInput()
    {
        if (!HasAvailableAbility())
            return false;

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
            return false;

        if (DialogueManager.Instance != null &&
            (DialogueManager.Instance.IsDialogueActive ||
             DialogueManager.Instance.IsGameplayInputBlocked()))
            return false;

        if (InventoryUI.Instance != null &&
            InventoryUI.Instance.IsInventoryActive())
            return false;

        if (GameplayUIManager.Instance != null &&
            GameplayUIManager.Instance.IsPuzzleViewActive)
            return false;

        return true;
    }

    private bool HasAvailableAbility()
    {
        return IsAvailable(RAbility.Sense) || IsAvailable(RAbility.Howl) ||
            IsAvailable(RAbility.Teleport);
    }

    private bool IsAvailable(RAbility ability)
    {
        AbilityManager manager = AbilityManager.Instance;
        if (manager == null)
            return false;

        return ability switch
        {
            RAbility.Sense => lunarSense != null &&
                manager.IsUnlocked(AbilityType.LunarSense),
            RAbility.Howl => howl != null &&
                manager.IsUnlocked(AbilityType.PurgeHowl),
            RAbility.Teleport => weave != null &&
                manager.IsUnlocked(AbilityType.MoonWeave),
            _ => false
        };
    }

    private void OpenWheel()
    {
        if (view == null)
            return;

        isWheelOpen = true;
        highlightedAbility = null;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        view.ShowWheel();
        view.SetHighlightedAbility(string.Empty);
    }

    private void UpdateWheelSelection()
    {
        Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 direction = (Vector2)Input.mousePosition - center;

        if (direction.magnitude < SelectionDeadZone)
        {
            highlightedAbility = null;
            view?.SetHighlightedAbility(string.Empty);
            return;
        }

        direction.Normalize();
        RAbility? closest = null;
        float closestDot = DirectionThreshold;

        TrySelectDirection(RAbility.Howl, Vector2.up, direction, ref closest, ref closestDot);
        TrySelectDirection(RAbility.Sense, Vector2.left, direction, ref closest, ref closestDot);
        TrySelectDirection(RAbility.Teleport, Vector2.right, direction, ref closest, ref closestDot);

        highlightedAbility = closest;
        view?.SetHighlightedAbility(
            closest.HasValue ? GetDisplayName(closest.Value) : string.Empty);
    }

    private void TrySelectDirection(
        RAbility ability,
        Vector2 abilityDirection,
        Vector2 cursorDirection,
        ref RAbility? closest,
        ref float closestDot)
    {
        if (!IsAvailable(ability))
            return;

        float dot = Vector2.Dot(abilityDirection, cursorDirection);
        if (dot <= closestDot)
            return;

        closest = ability;
        closestDot = dot;
    }

    private void CommitWheelSelection()
    {
        if (highlightedAbility.HasValue)
            selectedAbility = highlightedAbility.Value;

        CloseWheel();
        RefreshAvailability();
    }

    private void ActivateSelectedAbility()
    {
        if (!IsAvailable(selectedAbility))
            return;

        switch (selectedAbility)
        {
            case RAbility.Sense:
                lunarSense.ActivateSense();
                break;
            case RAbility.Howl:
                howl.ActivateHowl();
                break;
            case RAbility.Teleport:
                weave.OpenDestinationPrompt();
                break;
        }
    }

    private void CancelInput()
    {
        if (!isPressed && !isWheelOpen)
            return;

        isPressed = false;
        CloseWheel();
    }

    private void CloseWheel()
    {
        if (!isWheelOpen)
            return;

        isWheelOpen = false;
        highlightedAbility = null;
        view?.HideWheel();
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
    }

    private static string GetDisplayName(RAbility ability)
    {
        return ability switch
        {
            RAbility.Sense => "Sense",
            RAbility.Howl => "Howl",
            RAbility.Teleport => "Teleport",
            _ => string.Empty
        };
    }

    private void OnDisable()
    {
        CancelInput();
    }

    private void OnDestroy()
    {
        view?.Dispose();
    }
}
