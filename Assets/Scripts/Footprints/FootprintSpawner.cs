using UnityEngine;

public class FootprintSpawner : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerTransformation transformation;

    [Header("Movement Check")]
    [SerializeField] private float movementThreshold = 0.5f;

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
    public TerrainLayer snowTerrainLayer;
    public float rayDistance = 0.25f;

    [Header("Optional FX")]
    public ParticleSystem snowPuff;

    private void Awake()
    {
        if (transformation == null)
        {
            transformation = FindFirstObjectByType<PlayerTransformation>();
        }
    }

    // =====================================================
    // MOVEMENT CHECK
    // =====================================================

    private bool IsActuallyMoving()
    {
        Animator anim =
            transformation.currentForm == PlayerTransformation.FormState.Human
            ? transformation.HumanAnimator
            : transformation.WolfAnimator;

        if (anim == null)
            return false;

        return anim.GetFloat("Speed") > movementThreshold;
    }

    // =====================================================
    // HUMAN EVENTS
    // =====================================================

    public void HumanLeftStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Human)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(humanLeftFoot, humanLeftFootprint);
    }

    public void HumanRightStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Human)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(humanRightFoot, humanRightFootprint);
    }

    // =====================================================
    // WOLF EVENTS
    // =====================================================

    public void WolfFrontLeftStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Wolf)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(wolfFrontLeft, wolfFootprint);
    }

    public void WolfFrontRightStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Wolf)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(wolfFrontRight, wolfFootprint);
    }

    public void WolfBackLeftStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Wolf)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(wolfBackLeft, wolfFootprint);
    }

    public void WolfBackRightStep()
    {
        if (transformation.currentForm != PlayerTransformation.FormState.Wolf)
            return;

        if (!IsActuallyMoving())
            return;

        SpawnFootprintFromBone(wolfBackRight, wolfFootprint);
    }

    // =====================================================
    // SPAWNING
    // =====================================================

    private void SpawnFootprintFromBone(Transform foot, GameObject prefab)
    {
        if (foot == null || prefab == null)
            return;

        if (Physics.Raycast(
            foot.position + Vector3.up * 0.1f,
            Vector3.down,
            out RaycastHit hit,
            rayDistance))
        {
            if (IsSnowTerrain(hit))
            {
                Spawn(hit, prefab);
            }
        }
    }

    private void Spawn(RaycastHit hit, GameObject prefab)
    {
        Quaternion spawnRotation =
            Quaternion.LookRotation(-hit.normal, transform.forward);

        GameObject obj = Instantiate(
            prefab,
            hit.point + hit.normal * 0.01f,
            spawnRotation
        );

        if (snowPuff != null)
        {
            Instantiate(
                snowPuff,
                hit.point,
                Quaternion.identity
            );
        }

        AudioManager.Instance?.PlaySFX("WalkSnow");

        if (FootprintManager.Instance != null)
        {
            FootprintManager.Instance.RegisterFootprint(obj);
        }
    }

    private bool IsSnowTerrain(RaycastHit hit)
    {
        Terrain terrain = hit.collider.GetComponent<Terrain>();

        if (terrain == null)
            return false;

        TerrainData terrainData = terrain.terrainData;

        Vector3 terrainPos = hit.point - terrain.transform.position;

        int mapX = Mathf.FloorToInt(
            terrainPos.x / terrainData.size.x * terrainData.alphamapWidth);

        int mapZ = Mathf.FloorToInt(
            terrainPos.z / terrainData.size.z * terrainData.alphamapHeight);

        float[,,] splatmapData =
            terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int dominantLayer = 0;
        float maxMix = 0;

        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            float mix = splatmapData[0, 0, i];

            if (mix > maxMix)
            {
                maxMix = mix;
                dominantLayer = i;
            }
        }

        return terrainData.terrainLayers[dominantLayer] == snowTerrainLayer;
    }
}