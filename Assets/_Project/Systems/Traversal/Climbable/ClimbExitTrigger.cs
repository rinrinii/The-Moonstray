/*using UnityEngine;

public class ClimbExitTrigger : MonoBehaviour
{
    [Header("Exit Settings")]
    public float topHeight = 2f;

    public float forwardPush = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerClimbing climbing =
            other.GetComponent<PlayerClimbing>();

        if (climbing != null &&
            climbing.IsClimbing())
        {
            Debug.Log(
                "Top Exit Trigger Detected"
            );

            climbing.ExitClimbAtTop(
                topHeight,
                forwardPush
            );
        }
    }
}*/