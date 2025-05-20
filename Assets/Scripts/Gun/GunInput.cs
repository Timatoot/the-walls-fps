using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GunServer), typeof(GunFx))]
public class GunInput : NetworkBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform muzzle;

    [Header("Tuning")]
    [SerializeField] float fireRate = 8f;
    [SerializeField] float recoilKick = 0.24f;        // degrees up per shot
    [SerializeField] float recoverSpeed = 10f;        // degrees/s return

    GunServer server;
    GunFx fx;

    float nextShotTime;
    float totalKick;
    bool triggerLast;
    Coroutine recoverCo;

    void Awake()
    {
        server = GetComponent<GunServer>();
        fx = GetComponent<GunFx>();
    }

    void Update()
    {
        if (!IsOwner) return;

        bool trigger = Mouse.current.leftButton.isPressed;

        if (trigger && Time.time >= nextShotTime)
        {
            if (!triggerLast && recoverCo != null)
                StopCoroutine(recoverCo);

            nextShotTime = Time.time + 1f / fireRate;

            FireShot();
        }

        if (!trigger && triggerLast && totalKick > 0f)
            recoverCo = StartCoroutine(RecoverRecoil());

        triggerLast = trigger;
    }

    void FireShot()
    {
        Camera cam = Camera.main;

        Ray camRay = cam.ScreenPointToRay(
                      new Vector3(Screen.width * .5f, Screen.height * .5f, 0));

        Vector3 dst = camRay.origin + camRay.direction * server.Range;
        if (Physics.Raycast(camRay, out var hit, server.Range, server.HitMask))
            dst = hit.point;

        Vector3 src = muzzle.position;
        Vector3 dir = (dst - src).normalized;

        fx.PlayMuzzleFlash();
        fx.SpawnTracer(src, dst);

        var root = transform.root.Find("PlayerCameraRoot");
        if (root)
        {
            root.localRotation *= Quaternion.Euler(-recoilKick, 0, 0);
            totalKick += recoilKick;
        }

        server.ShootServerRpc(src, dir);
    }

    System.Collections.IEnumerator RecoverRecoil()
    {
        var root = transform.root.Find("PlayerCameraRoot");
        if (!root) yield break;

        while (totalKick > 0f)
        {
            if (Mouse.current.leftButton.isPressed) yield break;

            float step = recoverSpeed * Time.deltaTime;
            step = Mathf.Min(step, totalKick);

            root.localRotation *= Quaternion.Euler(step, 0f, 0f);
            totalKick -= step;

            yield return null;
        }
    }
}
