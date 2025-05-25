using Unity.Netcode;
using UnityEngine;

public class WeaponLayerSetter : NetworkBehaviour
{
    [SerializeField] Transform weaponRoot;

    void Start()
    {
        int layer = IsLocalPlayer
                    ? LayerMask.NameToLayer("LocalWeapon")
                    : LayerMask.NameToLayer("RemoteWeapon");

        SetLayerRecursively(weaponRoot, layer);
    }

    static void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform c in t) SetLayerRecursively(c, layer);
    }
}
