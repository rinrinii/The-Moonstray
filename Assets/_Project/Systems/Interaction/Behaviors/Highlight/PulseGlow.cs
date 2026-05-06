using UnityEngine;

public class PulseGlow : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1f;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = Mathf.Lerp(
            minScale,
            maxScale,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f
        );

        transform.localScale = baseScale * pulse;
    }
}