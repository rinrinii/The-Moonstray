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
        footprintSpawner?.WolfFrontLeftStep();
    }

    public void WolfFrontRightStep()
    {
        footprintSpawner?.WolfFrontRightStep();
    }

    public void WolfBackLeftStep()
    {
        footprintSpawner?.WolfBackLeftStep();
    }

    public void WolfBackRightStep()
    {
        footprintSpawner?.WolfBackRightStep();
    }
}