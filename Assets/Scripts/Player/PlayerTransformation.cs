using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerTransformation : MonoBehaviour
{
    public enum FormState { Human, Wolf }
    public FormState currentForm = FormState.Human;

    [Header("Models")]
    public GameObject humanModel;
    public GameObject wolfModel;

    [Header("Animators")]
    public Animator HumanAnimator;
    public Animator WolfAnimator;

    [Header("Movement")]
    public float humanSpeed = 5f;
    public float wolfSpeed = 9f;

    [Header("Collider Settings")]
    public float humanHeight = 1.8f;
    public float wolfHeight = 1.0f;

    [Header("World Effects")]
    public Light sunLight;
    public Vector2 dayRotation = new Vector2(120f, 150f);
    public Vector2 nightRotation = new Vector2(356f, 330f);
    public Volume dayVol;
    public Volume nightVol;
    public float transitionDuration = 1.5f;

    [Header("VFX")]
    public ParticleSystem transformParticles;
    public Light transformLight;
    public GameObject glowSphere;
    public float maxLightIntensity = 20f;
    public float maxSphereScale = 3f;

    private float currentSpeed;
    private CharacterController controller;
    private bool isTransitioning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        ApplyHumanForm();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isTransitioning)
        {
            StartCoroutine(TransformationSequence());
        }
    }

    // ✔ IMPORTANT: movement script uses this
    public bool CanMove()
    {
        return !isTransitioning;
    }

    IEnumerator TransformationSequence()
    {
        isTransitioning = true;
        bool turningToWolf = (currentForm == FormState.Human);

        float time = 0f;
        bool swapped = false;

        Quaternion startRot = sunLight.transform.rotation;
        Vector2 targetV2 = turningToWolf ? nightRotation : dayRotation;
        Quaternion endRot = Quaternion.Euler(targetV2.x, targetV2.y, 0f);

        float startDayWeight = dayVol.weight;
        float startNightWeight = nightVol.weight;

        // ======================================================
        // 1. START TRANSFORM (Locomotion → Rest)
        // ======================================================
        if (turningToWolf)
            HumanAnimator.SetBool("IsTransforming", true);
        else
            WolfAnimator.SetBool("IsTransforming", true);

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;

            // LIGHT
            if (sunLight)
                sunLight.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            // VOLUME
            if (dayVol)
                dayVol.weight = Mathf.Lerp(startDayWeight, turningToWolf ? 0f : 1f, t);

            if (nightVol)
                nightVol.weight = Mathf.Lerp(startNightWeight, turningToWolf ? 1f : 0f, t);

            // VFX BURST
            float burstCurve = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 10f);

            if (transformLight)
                transformLight.intensity = burstCurve * maxLightIntensity;

            if (glowSphere)
            {
                glowSphere.SetActive(true);
                glowSphere.transform.localScale = Vector3.one * (burstCurve * maxSphereScale);

                Material glowMat = glowSphere.GetComponent<MeshRenderer>().material;
                glowMat.SetColor("_EmissionColor", Color.white * (burstCurve * 50f));
            }

            // ======================================================
            // 2. SWAP (MID TRANSFORM)
            // ======================================================
            if (!swapped && time >= transitionDuration / 2f)
            {
                swapped = true;

                if (turningToWolf)
                {
                    ApplyWolfForm();
                    transformParticles?.Play();

                    GetComponent<PlayerMovement>().UpdateAnimator();

                    WolfAnimator.SetBool("IsTransforming", false);
                    WolfAnimator.Play("Rest–Reverse", 0, 0f);
                    StartCoroutine(ForceReturnToLocomotion(WolfAnimator));
                }
                else
                {
                    ApplyHumanForm();
                    transformParticles?.Play();

                    GetComponent<PlayerMovement>().UpdateAnimator();

                    HumanAnimator.SetBool("IsTransforming", false);
                    HumanAnimator.Play("Rest–Reverse", 0, 0f);
                    StartCoroutine(ForceReturnToLocomotion(HumanAnimator));
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ======================================================
        // 3. CLEANUP
        // ======================================================
        if (sunLight)
            sunLight.transform.rotation = endRot;

        if (transformLight)
            transformLight.intensity = 0f;

        if (glowSphere)
            glowSphere.SetActive(false);

        HumanAnimator.SetBool("IsTransforming", false);
        WolfAnimator.SetBool("IsTransforming", false);

        isTransitioning = false;
    }

    IEnumerator ForceReturnToLocomotion(Animator anim)
    {
        // wait for reverse animation timing (adjust if needed)
        yield return new WaitForSeconds(0.6f);

        anim.SetBool("IsTransforming", false);

        anim.CrossFade("Locomotion", 0.25f);
    }

    void ApplyHumanForm()
    {
        currentForm = FormState.Human;

        humanModel.SetActive(true);
        wolfModel.SetActive(false);

        currentSpeed = humanSpeed;
        controller.height = humanHeight;
        controller.center = new Vector3(0, humanHeight / 2f, 0);
    }

    void ApplyWolfForm()
    {
        currentForm = FormState.Wolf;

        humanModel.SetActive(false);
        wolfModel.SetActive(true);

        currentSpeed = wolfSpeed;
        controller.height = wolfHeight;
        controller.center = new Vector3(0, wolfHeight / 2f, 0);
    }

    public float GetSpeed()
    {
        return currentSpeed;
    }
}