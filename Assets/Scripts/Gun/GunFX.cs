using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class GunFx : NetworkBehaviour
{
    [Header("Prefabs / VFX")]
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] ParticleSystem hitSparkPrefab;
    [SerializeField] GameObject tracerPrefab;
    [SerializeField] ParticleSystem headshotSparkPrefab;

    [Header("Tracer")]
    [SerializeField] float tracerSpeed = 240f;       // m / s (fast = nearly hitscan)

    [Header("Audio")]
    [SerializeField] AudioSource shotSource;        // 3-D source, SpatialBlend = 1
    [SerializeField] AudioClip[] shotClips;
    [SerializeField] AudioClip emptyMagClip;
    [SerializeField] AudioClip reloadClip;
    [SerializeField] private AudioSource reloadSource;
    [SerializeField] private float reloadQuietFactor = 0.4f;

    public void PlayShotSound()
    {
        if (shotSource && shotClips.Length > 0)
        {
            var clip = shotClips[Random.Range(0, shotClips.Length)];
            shotSource.pitch = Random.Range(0.97f, 1.03f);
            shotSource.PlayOneShot(clip);
        }
    }

    public void PlayEmptyMag() { if (shotSource && emptyMagClip) shotSource.PlayOneShot(emptyMagClip); }
    public void PlayReload(bool quiet = false)
    {
        if (reloadSource && reloadClip)
        {
            reloadSource.volume = quiet ? reloadQuietFactor : 1f;
            reloadSource.PlayOneShot(reloadClip);
        }
    }

    public void PlayMuzzleFlash() { if (muzzleFlash) muzzleFlash.SetActive(true); }

    public void SpawnHitFx(Vector3 pos, Vector3 normal)
    {
        if (!hitSparkPrefab) return;
        var fx = Instantiate(hitSparkPrefab, pos, Quaternion.LookRotation(normal));
        Destroy(fx.gameObject, 1.5f);
    }

    public void SpawnHeadshotFx(Vector3 pos, Vector3 normal)
    {
        if (!headshotSparkPrefab) return;
        var spark = Instantiate(headshotSparkPrefab, pos, Quaternion.LookRotation(normal));
        Destroy(spark.gameObject, 1.5f);
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

        while (t < 1f && tf)
        {
            t += Time.deltaTime * tracerSpeed / dist;
            tf.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        if (tf) Destroy(tf.gameObject, 0.02f);
    }
}
