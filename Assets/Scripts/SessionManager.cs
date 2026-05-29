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
                // FIX: cooldownTimer is now a read-only shim on NetworkVariable.
                // Reading it (.Value via the shim) is still valid here — no change
                // needed on the READ side.
                cds[i] = abilities[i].cooldownTimer;

            savedStates[clientId] = new PlayerSavedState
            {
                health = ph.currentHealth.Value,
                cooldowns = cds
            };

            Debug.Log($"[SessionManager] Saved state for client {clientId} " +
                      $"— HP: {ph.currentHealth.Value}");
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
        // Wait a short time for the player object to finish spawning
        // before we try to find and modify it.
        yield return new WaitForSeconds(0.5f);

        var state = savedStates[clientId];

        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.OwnerClientId != clientId) continue;
            if (!netObj.TryGetComponent<PlayerHealth>(out var ph)) continue;

            // Restore health — direct NetworkVariable write is valid here
            // because RestoreState only runs on the server (called from
            // OnClientReconnect which is itself gated by OnNetworkSpawn's
            // !IsServer guard).
            ph.currentHealth.Value = state.health;

            var abilities = netObj.GetComponents<BaseAbility>();
            for (int i = 0; i < abilities.Length && i < state.cooldowns.Length; i++)
            {
                // FIX: was `abilities[i].cooldownTimer = state.cooldowns[i]`
                // cooldownTimer is now a read-only property (shim for the
                // NetworkVariable). Assignment won't compile.
                // SetCooldown() is the correct server-side write path —
                // it validates IsServer and writes _cooldownTimer.Value directly.
                abilities[i].SetCooldown(state.cooldowns[i]);
            }

            Debug.Log($"[SessionManager] Restored state for client {clientId} " +
                      $"— HP: {state.health}");
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