using Unity.Netcode;
using UnityEngine;

public class GunServer : NetworkBehaviour
{
    [Header("Ballistics")]
    [SerializeField] float damage = 25f;
    [SerializeField] float range = 60f;
    [SerializeField] LayerMask hitMask;

    public float Range => range;
    public LayerMask HitMask => hitMask;

    [ServerRpc]
    public void ShootServerRpc(Vector3 src, Vector3 dir, ServerRpcParams _ = default)
    {
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
