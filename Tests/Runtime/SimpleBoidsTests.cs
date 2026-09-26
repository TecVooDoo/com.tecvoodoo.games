// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class SimpleBoidsTests
    {
        const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null)
                    Object.DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }

        // SimpleBoids reads its serialized config in Awake, so the controller is built
        // inactive, configured through its private fields, then activated.
        SimpleBoids CreateBoids(int flocks, int boids, bool danger = false, int dangerMask = 0)
        {
            GameObject prefab = new GameObject("BoidPrefab");
            prefab.SetActive(false);
            spawned.Add(prefab);

            GameObject root = new GameObject("Boids");
            root.SetActive(false);
            spawned.Add(root);

            SimpleBoids boidsComponent = root.AddComponent<SimpleBoids>();
            SetField(boidsComponent, "boidPrefab", prefab);
            SetField(boidsComponent, "flockCount", flocks);
            SetField(boidsComponent, "boidCount", boids);
            SetField(boidsComponent, "scaleRange", new Vector2(1.0f, 1.5f));
            SetField(boidsComponent, "enableDanger", danger);
            SetField(boidsComponent, "dangerLayer", (LayerMask)dangerMask);

            root.SetActive(true);
            return boidsComponent;
        }

        static void SetField(object target, string name, object value)
        {
            FieldInfo field = typeof(SimpleBoids).GetField(name, InstanceFlags);
            Assert.That(field, Is.Not.Null, $"SimpleBoids has no field '{name}' -- test is out of date.");
            field.SetValue(target, value);
        }

        static T GetField<T>(object target, string name)
        {
            FieldInfo field = typeof(SimpleBoids).GetField(name, InstanceFlags);
            Assert.That(field, Is.Not.Null, $"SimpleBoids has no field '{name}' -- test is out of date.");
            return (T)field.GetValue(target);
        }

        static Quaternion ClampPitch(Quaternion rotation, float maxAngle)
        {
            MethodInfo method = typeof(SimpleBoids).GetMethod("ClampPitch", StaticFlags);
            Assert.That(method, Is.Not.Null, "SimpleBoids has no ClampPitch -- test is out of date.");
            return (Quaternion)method.Invoke(null, new object[] { rotation, maxAngle });
        }

        [Test]
        public void Awake_SpawnsOneChildPerBoidAndFlock()
        {
            SimpleBoids boids = CreateBoids(3, 12);

            Assert.That(boids.transform.childCount, Is.EqualTo(15));
            Assert.That(GetField<Transform[]>(boids, "boidTransforms").Length, Is.EqualTo(12));
            Assert.That(GetField<Transform[]>(boids, "flockTransforms").Length, Is.EqualTo(3));
        }

        [Test]
        public void Awake_WithDanger_AddsHiddenProbeChild()
        {
            SimpleBoids boids = CreateBoids(2, 5, danger: true);

            Assert.That(boids.transform.childCount, Is.EqualTo(8));
            Transform probe = GetField<Transform>(boids, "dangerProbeTransform");
            Assert.That(probe, Is.Not.Null);
            Assert.That(probe.parent, Is.EqualTo(boids.transform));
            Assert.That(probe.GetComponent<MeshRenderer>().enabled, Is.False);
        }

        [Test]
        public void Awake_FlockMarkersInactiveWithoutDebugGizmos()
        {
            SimpleBoids boids = CreateBoids(4, 4);

            foreach (Transform flock in GetField<Transform[]>(boids, "flockTransforms"))
                Assert.That(flock.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Awake_BoidsSpawnWithinScaleRangeAndLevel()
        {
            SimpleBoids boids = CreateBoids(1, 20);

            int[] assignment = GetField<int[]>(boids, "boidFlockAssignment");
            foreach (Transform boid in GetField<Transform[]>(boids, "boidTransforms"))
            {
                Assert.That(boid.parent, Is.EqualTo(boids.transform));
                Assert.That(boid.localScale.x, Is.InRange(1.0f, 1.5f));
                Assert.That(boid.localScale.x, Is.EqualTo(boid.localScale.y).And.EqualTo(boid.localScale.z));
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, boid.localEulerAngles.x)), Is.LessThan(1e-3f));
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, boid.localEulerAngles.z)), Is.LessThan(1e-3f));
            }
            foreach (int flock in assignment)
                Assert.That(flock, Is.EqualTo(0));
        }

        [Test]
        public void Awake_BoidSpeedsDrawnFromThreeToSeven()
        {
            SimpleBoids boids = CreateBoids(2, 50);

            foreach (float speed in GetField<float[]>(boids, "boidSpeeds"))
                Assert.That(speed, Is.InRange(3.0f, 7.0f));
        }

        [Test]
        public void ClampPitch_LimitsPitchAndZeroesRoll()
        {
            Quaternion clamped = ClampPitch(Quaternion.Euler(80f, 30f, 25f), 50f);
            Vector3 euler = clamped.eulerAngles;

            Assert.That(euler.x, Is.EqualTo(50f).Within(1e-3f));
            Assert.That(euler.y, Is.EqualTo(30f).Within(1e-3f));
            Assert.That(euler.z, Is.EqualTo(0f).Within(1e-3f));
        }

        // Negative pitch arrives from eulerAngles as 360 - n; the clamp must unwrap it first.
        [Test]
        public void ClampPitch_UnwrapsNegativePitch()
        {
            Quaternion clamped = ClampPitch(Quaternion.Euler(-70f, 0f, 0f), 50f);
            float pitch = clamped.eulerAngles.x;
            if (pitch > 180f)
                pitch -= 360f;

            Assert.That(pitch, Is.EqualTo(-50f).Within(1e-3f));
        }

        [Test]
        public void ClampPitch_LeavesInRangePitchUnchanged()
        {
            Quaternion clamped = ClampPitch(Quaternion.Euler(20f, 90f, 0f), 50f);

            Assert.That(clamped.eulerAngles.x, Is.EqualTo(20f).Within(1e-3f));
            Assert.That(clamped.eulerAngles.y, Is.EqualTo(90f).Within(1e-3f));
        }

        [UnityTest]
        public IEnumerator LateUpdate_MovesBoidsOverFrames()
        {
            SimpleBoids boids = CreateBoids(1, 6);
            Transform[] boidTransforms = GetField<Transform[]>(boids, "boidTransforms");
            Vector3[] start = new Vector3[boidTransforms.Length];
            for (int i = 0; i < boidTransforms.Length; i++)
                start[i] = boidTransforms[i].position;

            for (int frame = 0; frame < 5; frame++)
                yield return null;

            int moved = 0;
            for (int i = 0; i < boidTransforms.Length; i++)
            {
                if ((boidTransforms[i].position - start[i]).sqrMagnitude > 1e-8f)
                    moved++;
            }
            Assert.That(moved, Is.EqualTo(boidTransforms.Length));
        }

        [UnityTest]
        public IEnumerator BehaviorLoop_ReassignsBoidOffsets()
        {
            SimpleBoids boids = CreateBoids(1, 8);
            SetField(boids, "behaviorChangeInterval", new Vector2(0.01f, 0.02f));
            // The loop already drew its first delay from the default interval in Awake,
            // so restart it on the short one.
            boids.StopAllCoroutines();
            MethodInfo loop = typeof(SimpleBoids).GetMethod("BehavioralChangeLoop", InstanceFlags);
            Assert.That(loop, Is.Not.Null, "SimpleBoids has no BehavioralChangeLoop -- test is out of date.");
            boids.StartCoroutine((IEnumerator)loop.Invoke(boids, null));

            Vector3[] before = (Vector3[])GetField<Vector3[]>(boids, "boidOffsets").Clone();

            yield return new WaitForSeconds(0.25f);

            Vector3[] after = GetField<Vector3[]>(boids, "boidOffsets");
            int changed = 0;
            for (int i = 0; i < before.Length; i++)
            {
                if (before[i] != after[i])
                    changed++;
            }
            Assert.That(changed, Is.GreaterThan(0));
        }

        [Test]
        public void DangerLoop_NoThreat_KeepsNormalMultipliers()
        {
            SimpleBoids boids = CreateBoids(1, 3, danger: true, dangerMask: 0);

            Assert.That(GetField<float>(boids, "activeDangerSpeed"), Is.EqualTo(1f));
            Assert.That(GetField<float>(boids, "activeDangerTurn"), Is.EqualTo(1f));
        }
    }
}
