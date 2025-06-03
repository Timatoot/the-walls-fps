using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.Netcode;

[RequireComponent(typeof(GunServer), typeof(GunFx))]
public class GunInput : NetworkBehaviour
{
    [Header("Visual Shake")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    [Header("Real Aim Kick")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    private CinemachinePanTilt panTilt;
    [SerializeField] private GunMovement gunMovement;

    [Header("References")]
    [SerializeField] private Transform muzzle;

    [Header("Tuning")]
    [SerializeField] private float fireRate = 10f;
    [SerializeField] private float recoilKick = 0.5f; // degrees up
    [SerializeField] private float recoverSpeed = 6f;   // deg/sec down

    private GunServer server;
    private GunFx fx;
    private float nextShotTime;
    private float totalKick;
    private bool triggerLast;
    private Coroutine recoverCo;
    private int predictedAmmo;

    public int CurrentMag => predictedAmmo;

    void Awake()
    {
        server = GetComponent<GunServer>();
        fx = GetComponent<GunFx>();

        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();

        if (cinemachineCamera == null)
            cinemachineCamera = UnityEngine.Object
                .FindFirstObjectByType<CinemachineCamera>();

        panTilt = cinemachineCamera
            .GetComponent<CinemachinePanTilt>();

        predictedAmmo = server.AmmoInMag.Value;
        server.AmmoInMag.OnValueChanged += (_, srv) => predictedAmmo = srv;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current.rKey.wasPressedThisFrame && 
            !server.Reloading.Value && 
            server.AmmoInMag.Value != server.MaxAmmoInMag.Value)
        {
            StartCoroutine(LocalReloadDisplay());
            fx.PlayReload();
            server.ReloadServerRpc();
        }

        bool trigger = Mouse.current.leftButton.isPressed;

        if (trigger &&
        Time.time >= nextShotTime &&
        server.AmmoInMag.Value > 0 &&
        !server.Reloading.Value)
        {
            if (!triggerLast && recoverCo != null)
                StopCoroutine(recoverCo);

            nextShotTime = Time.time + 1f / fireRate;
            FireShot();
        }
        else if (trigger &&
        server.AmmoInMag.Value == 0 &&
        !server.Reloading.Value)
        {
            fx.PlayReload();
            StartCoroutine(LocalReloadDisplay());
            server.ReloadServerRpc();
        }
        else if (!trigger && triggerLast && totalKick > 0f)
        {
            recoverCo = StartCoroutine(RecoverRecoil());
        }

        triggerLast = trigger;
    }

    void FireShot()
    {
        Camera cam = Camera.main;

        Ray camRay = cam.ScreenPointToRay(new Vector3(
            Screen.width * 0.5f,
            Screen.height * 0.5f,
            0f
        ));

        Vector3 dst = camRay.origin + camRay.direction * server.Range;
        if (Physics.Raycast(camRay, out var hit, server.Range, server.HitMask))
            dst = hit.point;

        Vector3 src = muzzle.position;
        Vector3 dir = (dst - src).normalized;

        fx.PlayMuzzleFlash();
        fx.PlayShotSound();
        fx.SpawnTracer(src, dst);

        predictedAmmo--;

        gunMovement.Fire();

        impulseSource.GenerateImpulse();

        panTilt.TiltAxis.Value -= recoilKick;
        totalKick += recoilKick;

        server.ShootServerRpc(src, dir);
    }

    IEnumerator RecoverRecoil()
    {
        while (totalKick > 0f)
        {
            if (Mouse.current.leftButton.isPressed)
                yield break;

            float step = Mathf.Min(totalKick, recoverSpeed * Time.deltaTime);
            panTilt.TiltAxis.Value += step;
            totalKick -= step;

            yield return null;
        }
    }

    IEnumerator LocalReloadDisplay()
    {
        float t = 0f;
        while (t < server.ReloadTime)
        {
            t += Time.deltaTime;    
            yield return null;
        }
        predictedAmmo = server.MagazineSize;
    }
}
