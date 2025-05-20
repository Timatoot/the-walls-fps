using Unity.Netcode;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerSetup : NetworkBehaviour
{
    [SerializeField] private GameObject playerVirtualCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            DisableLocalControl();
        }
        else
        {
            EnableLocalControl();
        }
    }

    private void DisableLocalControl()
    {
        GetComponent<PlayerInput>().enabled = false;
        GetComponent<FirstPersonController>().enabled = false;

        foreach (var a in GetComponentsInChildren<AudioListener>(true))
            a.enabled = false;

        foreach (var c in GetComponentsInChildren<Camera>(true))
            c.enabled = false;

        if (playerVirtualCamera) playerVirtualCamera.SetActive(false);
    }

    private void EnableLocalControl()
    {
        if (playerVirtualCamera) playerVirtualCamera.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
