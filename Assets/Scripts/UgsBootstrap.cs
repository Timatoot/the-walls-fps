using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>Keep this in the first scene.</summary>
public class UgsBootstrap : MonoBehaviour
{
    async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
}
