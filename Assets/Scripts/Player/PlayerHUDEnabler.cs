using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerHUDEnabler : NetworkBehaviour
{
    [SerializeField] Canvas canvas;

    public override void OnNetworkSpawn()
    {
        canvas.enabled = IsOwner;
    }
}
