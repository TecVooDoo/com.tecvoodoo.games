// TecVooDoo Games - Tests
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace TecVooDoo.Games.Tests
{
    [TestFixture]
    public class BulletHoleSpawnerTests
    {
        readonly List<Object> spawned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null)
                    Object.DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }

        BulletHoleSpawner CreateSpawner(float fadeDuration)
        {
            GameObject go = new GameObject("BulletHoleSpawner");
            spawned.Add(go);
            BulletHoleSpawner spawner = go.AddComponent<BulletHoleSpawner>();
            spawner.fadeDuration = fadeDuration;
            spawner.decalSize = new Vector3(0.4f, 0.6f, 0.2f);
            return spawner;
        }

        static RaycastHit Hit(Vector3 point, Vector3 normal)
        {
            RaycastHit hit = new RaycastHit();
            hit.point = point;
            hit.normal = normal;
            return hit;
        }

        [Test]
        public void SpawnDecal_PlacesProjectorOffsetAlongNormal()
        {
            BulletHoleSpawner spawner = CreateSpawner(5f);
            Vector3 point = new Vector3(1f, 2f, 3f);

            spawner.SpawnDecal(Hit(point, Vector3.up));

            DecalProjector[] projectors = spawner.GetComponentsInChildren<DecalProjector>();
            Assert.That(projectors.Length, Is.EqualTo(1));
            DecalProjector dp = projectors[0];
            Assert.That(Vector3.Distance(dp.transform.position, point + Vector3.up * 0.01f), Is.LessThan(1e-4f));
            Assert.That(dp.size, Is.EqualTo(spawner.decalSize));
            Assert.That(Vector3.Angle(dp.transform.forward, Vector3.down), Is.LessThan(0.01f), "projects into the surface");
        }

        [Test]
        public void TrySpawnFromRay_HitsColliderAndMissesEmptySpace()
        {
            BulletHoleSpawner spawner = CreateSpawner(5f);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawned.Add(wall);
            wall.transform.position = new Vector3(0f, 0f, 10f);
            Physics.SyncTransforms();

            bool missed = spawner.TrySpawnFromRay(new Ray(Vector3.zero, Vector3.back));
            bool hit = spawner.TrySpawnFromRay(new Ray(Vector3.zero, Vector3.forward));

            Assert.That(missed, Is.False);
            Assert.That(hit, Is.True);
            DecalProjector dp = spawner.GetComponentInChildren<DecalProjector>();
            Assert.That(dp, Is.Not.Null);
            Assert.That(dp.transform.position.z, Is.EqualTo(9.49f).Within(0.02f), "cube face at z=9.5, offset 0.01 toward the ray");
        }

        [UnityTest]
        public IEnumerator Decal_FadesThenReturnsToPool()
        {
            BulletHoleSpawner spawner = CreateSpawner(0.5f);
            Time.captureDeltaTime = 0.125f;

            spawner.SpawnDecal(Hit(Vector3.zero, Vector3.forward));
            DecalProjector dp = spawner.GetComponentInChildren<DecalProjector>();
            Assert.That(dp.gameObject.activeSelf, Is.True);

            yield return null;
            yield return null;
            Assert.That(dp.fadeFactor, Is.LessThan(1f).And.GreaterThan(0f), "mid-fade");

            for (int i = 0; i < 10 && dp.gameObject.activeSelf; i++)
                yield return null;

            Assert.That(dp.gameObject.activeSelf, Is.False, "released to the pool");
            Assert.That(dp.fadeFactor, Is.EqualTo(1f), "fade restored for reuse");
        }

        [UnityTest]
        public IEnumerator ReleasedDecal_IsReusedByNextSpawn()
        {
            BulletHoleSpawner spawner = CreateSpawner(0.25f);
            Time.captureDeltaTime = 0.125f;

            spawner.SpawnDecal(Hit(Vector3.zero, Vector3.up));
            DecalProjector first = spawner.GetComponentInChildren<DecalProjector>();
            for (int i = 0; i < 10 && first.gameObject.activeSelf; i++)
                yield return null;
            Assert.That(first.gameObject.activeSelf, Is.False);

            spawner.SpawnDecal(Hit(Vector3.one, Vector3.up));

            DecalProjector[] all = spawner.GetComponentsInChildren<DecalProjector>(true);
            Assert.That(all.Length, Is.EqualTo(1), "pool reused the released projector");
            Assert.That(all[0], Is.SameAs(first));
            Assert.That(first.gameObject.activeSelf, Is.True);
        }
    }
}
