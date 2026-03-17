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
    /// Spawns URP DecalProjector bullet holes at raycast hit points using an object pool.
    /// Decals fade out over time and are returned to the pool automatically.
    /// Attach to any GameObject. Fires on left mouse click by default.
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
        Camera cam;

        void Start()
        {
            cam = Camera.main;
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

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.SphereCast(ray, decalSize.x * 0.3f, out RaycastHit hitInfo, Mathf.Infinity, decalLayers))
                {
                    SpawnDecal(hitInfo);
                }
            }
        }

        void SpawnDecal(RaycastHit hit)
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
