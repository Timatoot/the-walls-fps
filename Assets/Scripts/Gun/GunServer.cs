using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GunServer : NetworkBehaviour
{
    [Header("Ammo")]
    [SerializeField] int magazineSize = 30;
    [SerializeField] int maxReserve = 900;
    [SerializeField] float reloadTime = 2.5f;

    [Header("Ballistics")]
    [SerializeField] float damage = 25f;
    [SerializeField] float range = 60f;
    [SerializeField] LayerMask hitMask;

    public NetworkVariable<int> AmmoInMag { get; } = new();
    public NetworkVariable<int> AmmoReserve { get; } = new();
    public NetworkVariable<bool> Reloading { get; } = new();

    public float Range => range;
    public LayerMask HitMask => hitMask;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AmmoInMag.Value = magazineSize;
            AmmoReserve.Value = maxReserve;
            Reloading.Value = false;
        }
    }

    bool TryConsumeAmmo()
    {
        if (Reloading.Value || AmmoInMag.Value <= 0) return false;
        AmmoInMag.Value--;
        return true;
    }

    [ServerRpc]
    public void ReloadServerRpc()
    {
        if (Reloading.Value) return;
        if (AmmoInMag.Value == magazineSize) return;
        if (AmmoReserve.Value == 0) return;
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        Reloading.Value = true;
        yield return new WaitForSeconds(reloadTime);

        int need = magazineSize - AmmoInMag.Value;
        int toLoad = Mathf.Clamp(need, 0, AmmoReserve.Value);

        AmmoInMag.Value += toLoad;
        AmmoReserve.Value -= toLoad;
        Reloading.Value = false;
    }

    [ServerRpc]
    public void ShootServerRpc(Vector3 src, Vector3 dir, ServerRpcParams _ = default)
    {
        if (!TryConsumeAmmo()) return;

        Vector3 end = src + dir * range;

        if (Physics.Raycast(src, dir, out var hit, range, hitMask))
        {
            end = hit.point;
            if (hit.collider.GetComponentInParent<PlayerHealth>() is { } hp)
                hp.ApplyDamage(damage);
        }

        ShootClientRpc(src, end);
    }

    [ClientRpc]
    void ShootClientRpc(Vector3 src, Vector3 end, ClientRpcParams _ = default)
    {
        if (IsOwner) return;
        var fx = GetComponent<GunFx>();
        fx.SpawnTracer(src, end);
        fx.SpawnHitFx(end, (end - src).normalized);
    }
}
