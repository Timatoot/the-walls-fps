using TMPro;
using Unity.Netcode;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] GunServer gun;
    TMP_Text txt;

    void Awake() => txt = GetComponent<TMP_Text>();

    void Update()
    {
        if (!gun || !gun.IsSpawned) return;
        if (gun.Reloading.Value)
            txt.text = "RELOADING…";
        else
            txt.text = $"{gun.AmmoInMag.Value}  /  {gun.AmmoReserve.Value}";
    }
}
