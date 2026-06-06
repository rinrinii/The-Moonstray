using UnityEngine;

public class FootstepEventRelay : MonoBehaviour
{
    private FootprintSpawner footprintSpawner;

    private void Awake()
    {
        footprintSpawner = GetComponentInParent<FootprintSpawner>();
    }

    public void HumanLeftStep()
    {
        footprintSpawner?.HumanLeftStep();
    }

    public void HumanRightStep()
    {
        footprintSpawner?.HumanRightStep();
    }

    public void WolfFrontLeftStep()
    {
        Debug.Log("EVENT: Wolf Front Left");
        footprintSpawner?.WolfFrontLeftStep();
    }

    public void WolfFrontRightStep()
    {
        Debug.Log("EVENT: Wolf Front Right");
        footprintSpawner?.WolfFrontRightStep();
    }

    public void WolfBackLeftStep()
    {
        Debug.Log("EVENT: Wolf Back Left");
        footprintSpawner?.WolfBackLeftStep();
    }

    public void WolfBackRightStep()
    {
        Debug.Log("EVENT: Wolf Back Right");
        footprintSpawner?.WolfBackRightStep();
    }
}