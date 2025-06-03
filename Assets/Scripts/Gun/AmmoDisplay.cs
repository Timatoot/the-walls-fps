using TMPro;
using UnityEngine;
using Unity.Netcode;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] GunServer gun;
    [SerializeField] GunInput input;
    TMP_Text txt;

    void Awake() => txt = GetComponent<TMP_Text>();

    void Update()
    {
        if (!gun || !gun.IsSpawned) return;

        if (gun.Reloading.Value)
        {
            txt.text = "RELOADING…";
            return;
        }

        int mag = gun.IsOwner && input
                  ? input.CurrentMag
                  : gun.AmmoInMag.Value;

        txt.text = $"{mag}  /  {gun.AmmoReserve.Value}";
    }
}
