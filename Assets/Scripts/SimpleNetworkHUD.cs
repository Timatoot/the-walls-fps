using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SimpleNetworkHUD : MonoBehaviour
{
    string joinCode = "";

    void OnGUI()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        const int W = 160, H = 30;
        GUILayout.BeginArea(new Rect(10, 10, W + 20, H * 6));

        if (!nm.IsClient && !nm.IsServer)
        {

            if (GUILayout.Button("Host Online", GUILayout.Width(W), GUILayout.Height(H)))
            {
                _ = SessionManager.Instance.StartHostAsync().ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                        joinCode = t.Result;
                    else if (t.IsFaulted)
                        Debug.LogException(t.Exception);
                });
            }

            GUILayout.Space(5);

            joinCode = GUILayout.TextField(joinCode, 10, GUILayout.Width(W));
            if (GUILayout.Button("Join Online", GUILayout.Width(W), GUILayout.Height(H)))
            {
                if (!string.IsNullOrWhiteSpace(joinCode))
                    _ = SessionManager.Instance.JoinAsync(joinCode.Trim());
            }
        }
        else
        {
            GUILayout.Label(nm.IsHost ? $"Host  (code: {joinCode})"
                                      : nm.IsServer ? "Dedicated Server"
                                      : "Client");
            if (GUILayout.Button("Disconnect", GUILayout.Width(W), GUILayout.Height(H)))
            {
                _ = SessionManager.Instance.LeaveAsync();
                joinCode = "";
            }
        }

        GUILayout.EndArea();
    }
}
