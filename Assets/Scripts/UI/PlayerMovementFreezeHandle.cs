using UnityEngine;

public class PlayerMovementFreezeHandle
{
    private PlayerMovement playerMovement;
    private bool restoreOnRelease;

    public bool IsActive { get; private set; }

    public void Acquire()
    {
        if (IsActive)
            return;

        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        restoreOnRelease =
            playerMovement != null && playerMovement.enabled;

        if (playerMovement != null)
            playerMovement.enabled = false;

        IsActive = true;
    }

    public void Release()
    {
        if (!IsActive)
            return;

        if (restoreOnRelease && playerMovement != null)
            playerMovement.enabled = true;

        playerMovement = null;
        restoreOnRelease = false;
        IsActive = false;
    }
}
