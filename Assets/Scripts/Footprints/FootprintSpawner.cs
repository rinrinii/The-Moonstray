using UnityEngine;

public class FootprintSpawner : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerTransformation transformation;

    [Header("Human Feet")]
    public Transform humanLeftFoot;
    public Transform humanRightFoot;

    [Header("Wolf Paws")]
    public Transform wolfFrontLeft;
    public Transform wolfFrontRight;
    public Transform wolfBackLeft;
    public Transform wolfBackRight;

    [Header("Prefabs")]
    public GameObject humanLeftFootprint;
    public GameObject humanRightFootprint;
    public GameObject wolfFootprint;

    [Header("Ground")]
    public LayerMask snowLayer;
    public float rayDistance = 0.35f;

    [Header("Optional FX")]
    public ParticleSystem snowPuff;

    private bool hL, hR, wFL, wFR, wBL, wBR;

    void Awake()
    {
        if (transformation == null)
        {
            transformation = FindFirstObjectByType<PlayerTransformation>();
        }
    }

    void Update()
    {
        if (transformation == null)
            return;

        if (transformation.currentForm == PlayerTransformation.FormState.Wolf)
        {
            if (wolfFrontLeft) CheckFoot(wolfFrontLeft, ref wFL, wolfFootprint);
            if (wolfFrontRight) CheckFoot(wolfFrontRight, ref wFR, wolfFootprint);
            if (wolfBackLeft) CheckFoot(wolfBackLeft, ref wBL, wolfFootprint);
            if (wolfBackRight) CheckFoot(wolfBackRight, ref wBR, wolfFootprint);
        }
        else
        {
            if (humanLeftFoot) CheckFoot(humanLeftFoot, ref hL, humanLeftFootprint);
            if (humanRightFoot) CheckFoot(humanRightFoot, ref hR, humanRightFootprint);
        }
    }

    void CheckFoot(Transform foot, ref bool hasStepped, GameObject prefab = null)
    {
        if (foot == null) return;

        RaycastHit hit;

        bool grounded =
            Physics.Raycast(
                foot.position + Vector3.up * 0.1f,
                Vector3.down,
                out hit,
                rayDistance,
                snowLayer
            );

        if (grounded)
        {
            if (!hasStepped)
            {
                Spawn(hit, prefab);
                hasStepped = true;
            }
        }
        else
        {
            hasStepped = false;
        }
    }

    void Spawn(RaycastHit hit, GameObject prefab)
    {
        if (prefab == null) return;
        Quaternion spawnRotation = Quaternion.LookRotation(-hit.normal, transform.forward);

        GameObject obj = Instantiate(
            prefab,
            hit.point + hit.normal * 0.01f,
            spawnRotation
        );

        if (snowPuff != null)
        {
            Instantiate(snowPuff, hit.point, Quaternion.identity);
        }

        if (FootprintManager.Instance != null)
        {
            FootprintManager.Instance.RegisterFootprint(obj);
        }
    }
}