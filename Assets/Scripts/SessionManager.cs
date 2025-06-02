using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public ISession CurrentSession { get; private set; }

    [SerializeField] Camera menuCamera;

    void Awake()
    {
        if (Instance) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<string> StartHostAsync(int maxPlayers = 8)
    {
        var options = new SessionOptions
        {
            MaxPlayers = maxPlayers
        }.WithRelayNetwork();

        menuCamera.gameObject.SetActive(false);

        CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        return CurrentSession.Code;
    }

    public async Task JoinAsync(string joinCode)
    {
        menuCamera.gameObject.SetActive(false);

        CurrentSession = await MultiplayerService
                            .Instance.JoinSessionByCodeAsync(joinCode);
    }

    public async Task LeaveAsync()
    {
        if (CurrentSession != null)
        {
            await CurrentSession.LeaveAsync();
            CurrentSession = null;
        }
        NetworkManager.Singleton.Shutdown();
    }
}
