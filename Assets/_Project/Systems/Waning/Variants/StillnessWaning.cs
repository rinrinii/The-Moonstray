using UnityEngine;
using System.Collections;

public class StillnessWaning : WaningBase
{
    private Collider zoneCollider;
    private bool isDispersed = false;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
    }

    protected override void OnTriggerStay(Collider other)
    {
        // Safety guard: Stop damage calculations immediately when dispersed flag is true
        if (isDispersed) return;

        base.OnTriggerStay(other); 
    }

    /// <summary>
    /// Called by the Player's Q Howl code to clear this fog.
    /// </summary>
    public void Disperse()
    {
        if (isDispersed) return;
        StartCoroutine(DisperseRoutine());
    }

    private IEnumerator DisperseRoutine()
    {
        isDispersed = true;
        
        // Disable the collider so OnTriggerStay completely stops firing instantly
        if (zoneCollider != null)
        {
            zoneCollider.enabled = false; 
        }

        // Without particles, we can just delete the object immediately
        Destroy(gameObject);
        yield return null;
    }
}