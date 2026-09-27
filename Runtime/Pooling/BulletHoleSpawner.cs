// TecVooDoo Games
// Copyright (c) 2026 TecVooDoo LLC. All rights reserved.
// Based on BulletHoleSpawner by Adam Myhre (adammyhre)

using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

namespace TecVooDoo.Games
{
    /// <summary>
    /// Spawns URP DecalProjector bullet holes at hit points using an object pool.
    /// Decals fade out over time and are returned to the pool automatically.
    /// Input-agnostic: call <see cref="TrySpawnFromRay"/> or <see cref="SpawnDecal"/> from
    /// your own weapon / input code.
    /// </summary>
    public class BulletHoleSpawner : MonoBehaviour
    {
        [Tooltip("Material to use for the bullet hole decal.")]
        public Material decalMaterial;

        [Tooltip("Layers that can receive bullet hole decals.")]
        public LayerMask decalLayers = -1;

        [Tooltip("Size of each decal in world units (x = width, y = height, z = depth).")]
        public Vector3 decalSize = new Vector3(0.5f, 0.5f, 0.5f);

        [Tooltip("Duration in seconds for the decal to fade out before being returned to the pool.")]
        public float fadeDuration = 5f;

        IObjectPool<DecalProjector> decalPool;

        void Awake()
        {
            decalPool = new ObjectPool<DecalProjector>(
                createFunc: CreateDecal,
                actionOnGet: dp => dp.gameObject.SetActive(true),
                actionOnRelease: dp => dp.gameObject.SetActive(false),
                actionOnDestroy: dp => Destroy(dp.gameObject),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 20
            );
        }

        /// <summary>
        /// Sphere-casts along <paramref name="ray"/> against <see cref="decalLayers"/> and spawns
        /// a decal at the hit. Returns false when nothing was hit.
        /// </summary>
        public bool TrySpawnFromRay(Ray ray, float maxDistance = Mathf.Infinity)
        {
            if (!Physics.SphereCast(ray, decalSize.x * 0.3f, out RaycastHit hitInfo, maxDistance, decalLayers))
                return false;

            SpawnDecal(hitInfo);
            return true;
        }

        /// <summary>
        /// Spawns a decal at <paramref name="hit"/>, projected into the surface along its normal.
        /// </summary>
        public void SpawnDecal(RaycastHit hit)
        {
            DecalProjector projector = decalPool.Get();
            projector.transform.position = hit.point + hit.normal * 0.01f;
            Quaternion normalRotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            projector.transform.rotation = normalRotation * randomRotation;
            projector.size = decalSize;
            StartCoroutine(FadeAndRelease(projector, fadeDuration));
        }

        DecalProjector CreateDecal()
        {
            GameObject go = new GameObject("DecalProjector");
            DecalProjector dp = go.AddComponent<DecalProjector>();
            go.transform.parent = transform;
            dp.material = decalMaterial;
            dp.fadeFactor = 1f;
            dp.fadeScale = 0.95f;
            dp.startAngleFade = 0f;
            dp.endAngleFade = 30f;
            return dp;
        }

        IEnumerator FadeAndRelease(DecalProjector projector, float duration)
        {
            float time = 0f;
            float initialFade = projector.fadeFactor;
            while (time < duration)
            {
                if (projector == null) yield break;
                time += Time.deltaTime;
                projector.fadeFactor = Mathf.Lerp(initialFade, 0f, time / duration);
                yield return null;
            }
            if (projector != null)
            {
                projector.fadeFactor = initialFade;
                decalPool.Release(projector);
            }
        }
    }
}
