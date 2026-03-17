// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on #NVJOB Simple Boids v1.1.1 by Nicholas Veselov (MIT license)
// Original: https://nvjob.github.io/unity/nvjob-boids

using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Lightweight flocking simulation (boids). Spawns a configurable number of
    /// boid instances grouped into flocks that wander, migrate, and optionally
    /// react to danger via physics overlap. Works for birds, fish, butterflies,
    /// or any swarming entity -- assign the appropriate prefab.
    /// </summary>
    [AddComponentMenu("TecVooDoo/Simulation/Simple Boids")]
    public class SimpleBoids : MonoBehaviour
    {
        [Header("Behavior")]
        [Tooltip("Min/max seconds between behavioral changes (flock repositioning, speed shifts).")]
        [SerializeField] Vector2 behaviorChangeInterval = new Vector2(2.0f, 6.0f);

        [Tooltip("Show debug spheres at flock center points.")]
        [SerializeField] bool showDebugGizmos;

        [Header("Flocks")]
        [Range(1, 150)]
        [Tooltip("Number of independent flocks.")]
        [SerializeField] int flockCount = 2;

        [Range(0, 5000)]
        [Tooltip("Max distance flock centers wander from the controller origin.")]
        [SerializeField] int flockSpread = 30;

        [Range(0f, 1f)]
        [Tooltip("Vertical scale factor for flock center positions (0 = flat, 1 = full sphere).")]
        [SerializeField] float flockVerticalLimit = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("Chance per behavior change that a boid migrates to a different flock.")]
        [SerializeField] float migrationChance = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("Chance per behavior change that a flock center repositions.")]
        [SerializeField] float positionChangeChance = 0.5f;

        [Range(0f, 100f)]
        [Tooltip("SmoothDamp time for flock center movement. Lower = snappier.")]
        [SerializeField] float flockSmoothTime = 0.5f;

        [Header("Boids")]
        [Tooltip("Prefab to instantiate for each boid (bird, fish, butterfly, etc.).")]
        [SerializeField] GameObject boidPrefab;

        [Range(1, 9999)]
        [Tooltip("Total number of boid instances across all flocks.")]
        [SerializeField] int boidCount = 10;

        [Range(0f, 150f)]
        [Tooltip("Base forward speed of each boid.")]
        [SerializeField] float boidSpeed = 1f;

        [Range(0, 100)]
        [Tooltip("Max offset distance of individual boids from their flock center.")]
        [SerializeField] int boidSpread = 10;

        [Range(0f, 1f)]
        [Tooltip("Vertical scale factor for boid offset positions.")]
        [SerializeField] float boidVerticalLimit = 1f;

        [Range(0f, 10f)]
        [Tooltip("Turn rate toward target position. Higher = tighter turns.")]
        [SerializeField] float turnSpeed = 0.5f;

        [Range(0.01f, 500f)]
        [Tooltip("Amplitude of the vertical sine wave applied to all boids.")]
        [SerializeField] float verticalWaveAmplitude = 20f;

        [Tooltip("Clamp boid pitch to prevent extreme dive/climb angles.")]
        [SerializeField] bool clampRotation;

        [Range(0f, 360f)]
        [Tooltip("Max pitch angle when rotation clamping is enabled.")]
        [SerializeField] float maxRotationAngle = 50f;

        [Tooltip("Min/max random scale applied to each boid on spawn.")]
        [SerializeField] Vector2 scaleRange = new Vector2(1.0f, 1.5f);

        [Header("Danger")]
        [Tooltip("Enable threat detection. One flock reacts to nearby danger objects.")]
        [SerializeField] bool enableDanger;

        [Tooltip("Physics overlap radius for danger detection.")]
        [SerializeField] float dangerRadius = 15f;

        [Tooltip("Speed multiplier when fleeing from danger.")]
        [SerializeField] float dangerSpeedMultiplier = 1.5f;

        [Tooltip("Turn speed multiplier when fleeing from danger.")]
        [SerializeField] float dangerTurnMultiplier = 0.5f;

        [Tooltip("Layer mask for objects that trigger the danger response.")]
        [SerializeField] LayerMask dangerLayer;

        // Runtime state
        Transform cachedTransform;
        Transform dangerProbeTransform;
        int dangerTargetIndex;
        Transform[] boidTransforms;
        Transform[] flockTransforms;
        Vector3[] boidOffsets;
        Vector3[] flockPositions;
        Vector3[] flockVelocities;
        float[] boidSpeeds;
        float[] boidCurrentSpeeds;
        float[] speedDampVelocities;
        int[] boidFlockAssignment;
        float activeDangerSpeed;
        float activeDangerTurn;
        float elapsedTime;

        static WaitForSeconds dangerCheckDelay;

        void Awake()
        {
            cachedTransform = transform;
            InitializeFlocks();
            InitializeBoids();
            activeDangerSpeed = 1f;
            activeDangerTurn = 1f;
            StartCoroutine(BehavioralChangeLoop());
            StartCoroutine(DangerDetectionLoop());
        }

        void LateUpdate()
        {
            UpdateFlockPositions();
            UpdateBoidPositions();
        }

        void UpdateFlockPositions()
        {
            for (int f = 0; f < flockCount; f++)
            {
                Vector3 currentPos = flockTransforms[f].localPosition;
                flockTransforms[f].localPosition = Vector3.SmoothDamp(
                    currentPos, flockPositions[f], ref flockVelocities[f], flockSmoothTime);
            }
        }

        void UpdateBoidPositions()
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            Vector3 forwardStep = Vector3.forward * boidSpeed * activeDangerSpeed * deltaTime;
            float waveOffset = (verticalWaveAmplitude * 0.5f) - Mathf.PingPong(elapsedTime * 0.5f, verticalWaveAmplitude);
            Vector3 verticalWave = Vector3.up * waveOffset;
            float effectiveTurn = turnSpeed * activeDangerTurn * deltaTime;

            for (int b = 0; b < boidCount; b++)
            {
                if (boidCurrentSpeeds[b] != boidSpeeds[b])
                {
                    boidCurrentSpeeds[b] = Mathf.SmoothDamp(
                        boidCurrentSpeeds[b], boidSpeeds[b], ref speedDampVelocities[b], 0.5f);
                }

                boidTransforms[b].Translate(forwardStep * boidSpeeds[b]);

                Vector3 targetDirection = flockTransforms[boidFlockAssignment[b]].position
                    + boidOffsets[b] + verticalWave - boidTransforms[b].position;

                Quaternion targetRotation = Quaternion.LookRotation(
                    Vector3.RotateTowards(boidTransforms[b].forward, targetDirection, effectiveTurn, 0f));

                if (!clampRotation)
                {
                    boidTransforms[b].rotation = targetRotation;
                }
                else
                {
                    boidTransforms[b].localRotation = ClampPitch(targetRotation, maxRotationAngle);
                }
            }
        }

        IEnumerator DangerDetectionLoop()
        {
            if (!enableDanger)
            {
                activeDangerSpeed = 1f;
                activeDangerTurn = 1f;
                yield break;
            }

            dangerCheckDelay = new WaitForSeconds(1.0f);

            while (true)
            {
                if (Random.value > 0.9f)
                {
                    dangerTargetIndex = Random.Range(0, boidCount);
                }

                dangerProbeTransform.localPosition = boidTransforms[dangerTargetIndex].localPosition;

                if (Physics.CheckSphere(dangerProbeTransform.position, dangerRadius, dangerLayer))
                {
                    activeDangerSpeed = dangerSpeedMultiplier;
                    activeDangerTurn = dangerTurnMultiplier;
                    yield return dangerCheckDelay;
                }
                else
                {
                    activeDangerSpeed = 1f;
                    activeDangerTurn = 1f;
                }

                yield return dangerCheckDelay;
            }
        }

        IEnumerator BehavioralChangeLoop()
        {
            while (true)
            {
                float delay = Random.Range(behaviorChangeInterval.x, behaviorChangeInterval.y);
                yield return new WaitForSeconds(delay);

                for (int f = 0; f < flockCount; f++)
                {
                    if (Random.value < positionChangeChance)
                    {
                        Vector3 randomSphere = Random.insideUnitSphere * flockSpread;
                        flockPositions[f] = new Vector3(
                            randomSphere.x,
                            Mathf.Abs(randomSphere.y * flockVerticalLimit),
                            randomSphere.z);
                    }
                }

                for (int b = 0; b < boidCount; b++)
                {
                    boidSpeeds[b] = Random.Range(3.0f, 7.0f);
                    Vector3 randomOffset = Random.insideUnitSphere * boidSpread;
                    boidOffsets[b] = new Vector3(
                        randomOffset.x,
                        randomOffset.y * boidVerticalLimit,
                        randomOffset.z);

                    if (Random.value < migrationChance)
                    {
                        boidFlockAssignment[b] = Random.Range(0, flockCount);
                    }
                }
            }
        }

        void InitializeFlocks()
        {
            flockTransforms = new Transform[flockCount];
            flockPositions = new Vector3[flockCount];
            flockVelocities = new Vector3[flockCount];
            boidFlockAssignment = new int[boidCount];

            for (int f = 0; f < flockCount; f++)
            {
                GameObject flockMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flockMarker.SetActive(showDebugGizmos);
                flockTransforms[f] = flockMarker.transform;
                flockTransforms[f].position = cachedTransform.position;

                Vector3 randomSphere = Random.onUnitSphere * flockSpread;
                flockPositions[f] = new Vector3(
                    randomSphere.x,
                    Mathf.Abs(randomSphere.y * flockVerticalLimit),
                    randomSphere.z);

                flockTransforms[f].parent = cachedTransform;
            }

            if (enableDanger)
            {
                GameObject dangerProbe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                MeshRenderer dangerRenderer = dangerProbe.GetComponent<MeshRenderer>();
                dangerRenderer.enabled = showDebugGizmos;
                dangerProbe.layer = gameObject.layer;
                dangerProbeTransform = dangerProbe.transform;
                dangerProbeTransform.parent = cachedTransform;
            }
        }

        void InitializeBoids()
        {
            boidTransforms = new Transform[boidCount];
            boidSpeeds = new float[boidCount];
            boidCurrentSpeeds = new float[boidCount];
            boidOffsets = new Vector3[boidCount];
            speedDampVelocities = new float[boidCount];

            for (int b = 0; b < boidCount; b++)
            {
                boidTransforms[b] = Instantiate(boidPrefab, cachedTransform).transform;

                Vector3 randomOffset = Random.insideUnitSphere * boidSpread;
                Vector3 startOffset = new Vector3(
                    randomOffset.x,
                    randomOffset.y * boidVerticalLimit,
                    randomOffset.z);

                boidTransforms[b].localPosition = startOffset;
                boidOffsets[b] = startOffset;
                boidTransforms[b].localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
                boidTransforms[b].localRotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
                boidFlockAssignment[b] = Random.Range(0, flockCount);
                boidSpeeds[b] = Random.Range(3.0f, 7.0f);
            }
        }

        static Quaternion ClampPitch(Quaternion rotation, float maxAngle)
        {
            Vector3 euler = rotation.eulerAngles;
            float pitch = (euler.x > 180f) ? euler.x - 360f : euler.x;
            euler.x = Mathf.Clamp(pitch, -maxAngle, maxAngle);
            euler.z = 0f;
            return Quaternion.Euler(euler);
        }
    }
}
