using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
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
    public float wolfSpeed = 6.5f;

    private float speedModifier = 1f;
    private bool ignoreSpeedModifiers;

    [Header("Jump")]
    public float humanJumpHeight = 2f;
    public float wolfJumpHeight = 1.5f;

    [Header("Gravity")]
    public float humanGravity = -16f;
    public float wolfGravity = -20f;

    [Header("Collider Settings")]
    public float humanHeight = 1.8f;
    public float wolfHeight = 1.0f;

    [Header("World Effects")]
    public Light sunLight;
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
    private float currentJumpHeight;
    private float currentGravity;

    private CharacterController controller;
    private bool isTransitioning = false;

    // =========================
    // Unity Events
    // =========================

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        RefreshSceneReferences();

        ApplyHumanForm();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) &&
            !isTransitioning)
        {
            StartCoroutine(
                TransformationSequence()
            );
        }
    }

    // =========================
    // Scene References
    // =========================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        RefreshSceneReferences();
    }

    private void RefreshSceneReferences()
    {
        GameObject sun =
            GameObject.FindWithTag("Sun");

        if (sun != null)
        {
            sunLight =
                sun.GetComponent<Light>();
        }
        else
        {
            Debug.LogWarning(
                $"No object with 'Sun' tag found in scene: {SceneManager.GetActiveScene().name}"
            );
        }
    }

    // =========================
    // Public API
    // =========================

    public bool CanMove()
    {
        return !isTransitioning;
    }

    public float GetSpeed()
    {
        if (ignoreSpeedModifiers)
        {
            return currentSpeed;
        }

        return currentSpeed *
               speedModifier;
    }

    public void SetSpeedModifier(float modifier)
    {
        speedModifier = modifier;
    }

    public void SetIgnoreSpeedModifiers(bool ignore)
    {
        ignoreSpeedModifiers = ignore;
    }

    public float GetJumpHeight()
    {
        return currentJumpHeight;
    }

    public float GetGravity()
    {
        return currentGravity;
    }

    // =========================
    // Transformation
    // =========================

    private IEnumerator TransformationSequence()
    {
        isTransitioning = true;

        bool turningToWolf =
            currentForm == FormState.Human;

        float time = 0f;
        bool swapped = false;

        float spinAmount = 360f;

        float startDayWeight =
            dayVol.weight;

        float startNightWeight =
            nightVol.weight;

        if (turningToWolf)
            HumanAnimator.SetBool(
                "IsTransforming",
                true
            );
        else
            WolfAnimator.SetBool(
                "IsTransforming",
                true
            );

        while (time < transitionDuration)
        {
            float t =
                time / transitionDuration;

            if (sunLight)
            {
                sunLight.transform.Rotate(
                    0f,
                    (spinAmount /
                     transitionDuration) *
                    Time.deltaTime,
                    0f,
                    Space.World
                );
            }

            if (dayVol)
            {
                dayVol.weight =
                    Mathf.Lerp(
                        startDayWeight,
                        turningToWolf
                            ? 0f
                            : 1f,
                        t
                    );
            }

            if (nightVol)
            {
                nightVol.weight =
                    Mathf.Lerp(
                        startNightWeight,
                        turningToWolf
                            ? 1f
                            : 0f,
                        t
                    );
            }

            float burstCurve =
                Mathf.Pow(
                    Mathf.Sin(t * Mathf.PI),
                    10f
                );

            if (transformLight)
            {
                transformLight.intensity =
                    burstCurve *
                    maxLightIntensity;
            }

            if (glowSphere)
            {
                glowSphere.SetActive(true);

                glowSphere.transform.localScale =
                    Vector3.one *
                    (
                        burstCurve *
                        maxSphereScale
                    );

                Material glowMat =
                    glowSphere
                        .GetComponent<MeshRenderer>()
                        .material;

                glowMat.SetColor(
                    "_EmissionColor",
                    Color.white *
                    (
                        burstCurve * 50f
                    )
                );
            }

            if (!swapped &&
                time >=
                transitionDuration / 2f)
            {
                swapped = true;

                if (turningToWolf)
                {
                    ApplyWolfForm();

                    transformParticles?.Play();

                    GetComponent<PlayerMovement>()
                        .UpdateAnimator();

                    WolfAnimator.SetBool(
                        "IsTransforming",
                        false
                    );

                    WolfAnimator.Play(
                        "Rest–Reverse",
                        0,
                        0f
                    );

                    StartCoroutine(
                        ForceReturnToLocomotion(
                            WolfAnimator
                        )
                    );
                }
                else
                {
                    ApplyHumanForm();

                    transformParticles?.Play();

                    GetComponent<PlayerMovement>()
                        .UpdateAnimator();

                    HumanAnimator.SetBool(
                        "IsTransforming",
                        false
                    );

                    HumanAnimator.Play(
                        "Rest–Reverse",
                        0,
                        0f
                    );

                    StartCoroutine(
                        ForceReturnToLocomotion(
                            HumanAnimator
                        )
                    );
                }
            }

            time += Time.deltaTime;

            yield return null;
        }

        if (transformLight)
        {
            transformLight.intensity = 0f;
        }

        if (glowSphere)
        {
            glowSphere.SetActive(false);
        }

        HumanAnimator.SetBool(
            "IsTransforming",
            false
        );

        WolfAnimator.SetBool(
            "IsTransforming",
            false
        );

        isTransitioning = false;
    }

    private IEnumerator ForceReturnToLocomotion(
        Animator anim)
    {
        yield return new WaitForSeconds(
            0.6f
        );

        anim.SetBool(
            "IsTransforming",
            false
        );

        anim.CrossFade(
            "Locomotion",
            0.25f
        );
    }

    // =========================
    // Form Setup
    // =========================

    private void ApplyHumanForm()
    {
        currentForm = FormState.Human;

        humanModel.SetActive(true);
        wolfModel.SetActive(false);

        currentSpeed = humanSpeed;
        currentJumpHeight =
            humanJumpHeight;
        currentGravity =
            humanGravity;

        controller.height =
            humanHeight;

        controller.center =
            new Vector3(
                0,
                humanHeight / 2f,
                0
            );
    }

    private void ApplyWolfForm()
    {
        currentForm = FormState.Wolf;

        humanModel.SetActive(false);
        wolfModel.SetActive(true);

        currentSpeed = wolfSpeed;
        currentJumpHeight =
            wolfJumpHeight;
        currentGravity =
            wolfGravity;

        controller.height =
            wolfHeight;

        controller.center =
            new Vector3(
                0,
                wolfHeight / 2f,
                0
            );
    }
}