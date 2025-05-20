using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class GunFx : NetworkBehaviour
{
    [Header("Prefabs / VFX")]
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] ParticleSystem hitSparkPrefab;
    [SerializeField] GameObject tracerPrefab;

    [Header("Tracer")]
    [SerializeField] float tracerSpeed = 240f;       // m / s (fast = nearly hitscan)

    public void PlayMuzzleFlash()
    {
        if (muzzleFlash) muzzleFlash.Play();
    }

    public void SpawnHitFx(Vector3 pos, Vector3 normal)
    {
        if (!hitSparkPrefab) return;
        var fx = Instantiate(hitSparkPrefab, pos, Quaternion.LookRotation(normal));
        Destroy(fx.gameObject, 1.5f);
    }

    public void SpawnTracer(Vector3 origin, Vector3 hitPoint)
    {
        if (!tracerPrefab) return;

        var go = Instantiate(tracerPrefab, origin, Quaternion.identity);
        StartCoroutine(MoveTracer(go.transform, origin, hitPoint));
    }

    IEnumerator MoveTracer(Transform tf, Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        float t = 0f;

        while (t < 1f && tf) // abort if object destroyed
        {
            t += Time.deltaTime * tracerSpeed / dist;
            tf.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        if (tf) Destroy(tf.gameObject, 0.02f); // trail fade delay
    }
}
