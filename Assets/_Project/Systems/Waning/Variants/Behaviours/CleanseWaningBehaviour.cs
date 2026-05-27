using UnityEngine;

public class CleanseWaningBehaviour : MonoBehaviour, IObjectBehaviour
{
    [Header("References")]
    [SerializeField] private SpringWaningController waningController;

    public void Execute()
    {
        if (waningController == null)
        {
            Debug.LogWarning(
                "SpringWaningController missing."
            );

            return;
        }

        waningController.Cleanse();
    }
}