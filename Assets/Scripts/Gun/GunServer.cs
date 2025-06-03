using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GunServer : NetworkBehaviour
{
    [Header("Ammo")]
    [SerializeField] int magazineSize = 30;
    [SerializeField] const int maxMagazineSize = 30;
    [SerializeField] int maxReserve = 900;
    [SerializeField] float reloadTime = 2.5f;

    [Header("Ballistics")]
    [SerializeField] float damage = 25f;
    [SerializeField] float headshotDamage = 100f;
    [SerializeField] float range = 60f;
    [SerializeField] LayerMask hitMask;

    public NetworkVariable<int> AmmoInMag { get; } = new();
    public NetworkVariable<int> AmmoReserve { get; } = new();
    public NetworkVariable<bool> Reloading { get; } = new();

    public NetworkVariable<float> MaxAmmoInMag { get; } = new();

    public int MagazineSize => magazineSize;
    public int MaxReserve => maxReserve;
    public float ReloadTime => reloadTime;

    public float Range => range;
    public LayerMask HitMask => hitMask;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AmmoInMag.Value = magazineSize;
            AmmoReserve.Value = maxReserve;
            Reloading.Value = false;
            MaxAmmoInMag.Value = maxMagazineSize;
        }
    }

    [ServerRpc]
    public void ReloadServerRpc(ServerRpcParams rpc = default)
    {
        if (Reloading.Value || AmmoInMag.Value == magazineSize || AmmoReserve.Value == 0)
            return;

        PlayReloadClientRpc();

        StartCoroutine(ReloadRoutine());
    }

    [ClientRpc]
    private void PlayReloadClientRpc(ClientRpcParams _ = default)
    {
        if (IsOwner) return;

        var fx = GetComponent<GunFx>();
        if (!fx) return;

        var listener = Camera.main?.transform;
        if (listener && Vector3.Distance(listener.position, transform.position) <= 15f)
            fx.PlayReload(true);
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

    bool TryConsumeAmmo()
    {
        if (Reloading.Value || AmmoInMag.Value <= 0) return false;
        AmmoInMag.Value--;
        return true;
    }

    [ServerRpc]
    public void ShootServerRpc(Vector3 src, Vector3 dir, ServerRpcParams _ = default)
    {
        if (!TryConsumeAmmo()) return;

        const QueryTriggerInteraction trg = QueryTriggerInteraction.Collide;
        Vector3 end = src + dir * range;
        bool headshot = false;

        if (Physics.Raycast(src, dir, out var hit, range, hitMask, trg))
        {
            end = hit.point;

            if (hit.collider.TryGetComponent<Hitbox>(out var hb) && hb != null && hb.owner != null)
            {
                if (hb.owner.NetworkObject.OwnerClientId == OwnerClientId)
                    return;
                headshot = hb.isHead;
                float dmg = headshot ? headshotDamage : damage;
                if (dmg > hb.owner.Health.Value)
                    dmg = hb.owner.Health.Value;
                hb.owner.ApplyDamage(dmg);
            }
        }

        ShootClientRpc(src, end, headshot);
    }

    [ClientRpc]
    void ShootClientRpc(Vector3 src, Vector3 end, bool headshot, ClientRpcParams _ = default)
    {
        if (IsOwner) return;

        var fx = GetComponent<GunFx>();
        if (!fx) return;

        fx.PlayShotSound();
        fx.SpawnTracer(src, end);
        fx.SpawnHitFx(end, (end - src).normalized);

        if (headshot)
            fx.SpawnHeadshotFx(end, (end - src).normalized);
    }
}
