using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SessionManager : NetworkBehaviour
{
    private Dictionary<ulong, PlayerSavedState> savedStates = new();

    public struct PlayerSavedState
    {
        public float health;
        public float[] cooldowns;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientReconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.OwnerClientId != clientId) continue;
            if (!netObj.TryGetComponent<PlayerHealth>(out var ph)) continue;

            var abilities = netObj.GetComponents<BaseAbility>();
            float[] cds = new float[abilities.Length];
            for (int i = 0; i < abilities.Length; i++)
                cds[i] = abilities[i].cooldownTimer;

            savedStates[clientId] = new PlayerSavedState
            {
                health = ph.currentHealth.Value,
                cooldowns = cds
            };
            break;
        }
    }

    private void OnClientReconnect(ulong clientId)
    {
        if (!savedStates.ContainsKey(clientId)) return;
        StartCoroutine(RestoreState(clientId));
    }

    private IEnumerator RestoreState(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);
        var state = savedStates[clientId];
        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.OwnerClientId != clientId) continue;
            if (!netObj.TryGetComponent<PlayerHealth>(out var ph)) continue;

            ph.currentHealth.Value = state.health;
            var abilities = netObj.GetComponents<BaseAbility>();
            for (int i = 0; i < abilities.Length && i < state.cooldowns.Length; i++)
                abilities[i].cooldownTimer = state.cooldowns[i];
            break;
        }
        savedStates.Remove(clientId);
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientReconnect;
    }
}