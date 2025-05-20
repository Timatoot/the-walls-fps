using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float respawnDelay = 3f;

    public NetworkVariable<float> Health = new();

    CharacterController cc;
    bool isRespawning;

    void Awake() => cc = GetComponent<CharacterController>();

    public override void OnNetworkSpawn()
    {
        if (IsServer) Health.Value = maxHealth;


        if (IsClient)
        {
            var ui = Instantiate(Resources.Load<HealthBarUI>("HealthBar"));
            ui.Init(transform, maxHealth);
            Health.OnValueChanged += (_, v) => ui.OnHealthChanged(v);
        }
    }

    public void ApplyDamage(float dmg)
    {
        if (!IsServer || isRespawning) return;

        Health.Value = Mathf.Max(Health.Value - dmg, 0f);
        if (Health.Value <= 0f)
        {
            isRespawning = true;
            StartCoroutine(Server_Respawn());
        }
    }

    IEnumerator Server_Respawn()
    {
        var fpsCtl = GetComponent<StarterAssets.FirstPersonController>();

        cc.enabled = false;
        fpsCtl.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        Vector3 newPos = GetRandomSpawnPoint();
        TeleportClientRpc(newPos);
        Health.Value = maxHealth;

        cc.enabled = true;
        fpsCtl.enabled = true;
        isRespawning = false;
    }

    [ClientRpc]
    void TeleportClientRpc(Vector3 pos)
    {
        transform.position = pos;
    }

    Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-4, 4), 1f, Random.Range(-4, 4));
    }
}
