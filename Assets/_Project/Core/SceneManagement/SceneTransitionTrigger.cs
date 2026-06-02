using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField]
    private string targetScene;

    [SerializeField]
    private string targetSpawnID;

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SceneLoader.LoadScene(
            targetScene,
            targetSpawnID
        );
    }
}