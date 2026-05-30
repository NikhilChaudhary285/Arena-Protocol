using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }

    // Static — survives across SessionManager re-creations
    private static Dictionary<int, PlayerSavedState> savedSlots = new();
    private static Dictionary<ulong, int> clientSlotMap = new();
    private static int nextSlot = 0;

    public struct PlayerSavedState
    {
        public float health;
        public float[] cooldowns;
        public bool hasData;
    }

    private void Awake()
    {
        Debug.Log("[Session] Awake");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[Session] OnNetworkSpawn | IsServer={IsServer}");
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Debug.Log("[Session] Server callbacks registered");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // Skip host — host never reconnects
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"[Session] clientId={clientId} is host — skip");
            return;
        }

        Debug.Log($"[Session] OnClientConnected clientId={clientId} | " +
                  $"savedSlots.Count={savedSlots.Count} | " +
                  $"clientSlotMap.Count={clientSlotMap.Count}");

        // Check if there is ANY saved slot not currently mapped
        // to an active client — that means this is a reconnect
        int reconnectSlot = -1;
        foreach (var kvp in savedSlots)
        {
            // This saved slot is not currently in use by any active client
            if (!clientSlotMap.ContainsValue(kvp.Key))
            {
                reconnectSlot = kvp.Key;
                Debug.Log($"[Session] Found unoccupied saved slot " +
                          $"{reconnectSlot} — treating as reconnect");
                break;
            }
        }

        if (reconnectSlot >= 0)
        {
            // This is a RECONNECTING client — map them to the saved slot
            clientSlotMap[clientId] = reconnectSlot;
            Debug.Log($"[Session] Reconnect — mapped clientId={clientId} " +
                      $"to saved slot={reconnectSlot}");

            // Start restore
            StartCoroutine(RestoreStateWhenReady(clientId, reconnectSlot));
        }
        else
        {
            // This is a FRESH join — assign a new slot
            clientSlotMap[clientId] = nextSlot;
            Debug.Log($"[Session] Fresh join — mapped clientId={clientId} " +
                      $"to new slot={nextSlot}");
            nextSlot++;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        Debug.Log($"[Session] OnClientDisconnect clientId={clientId}");

        // Find and save player state
        foreach (var netObj in FindObjectsOfType<NetworkObject>())
        {
            if (netObj.OwnerClientId != clientId) continue;
            if (!netObj.TryGetComponent<PlayerHealth>(out var ph)) continue;

            var abilities = netObj.GetComponents<BaseAbility>();
            float[] cds = new float[abilities.Length];
            for (int i = 0; i < abilities.Length; i++)
                cds[i] = abilities[i].cooldownTimer;

            // Get the slot for this client
            if (!clientSlotMap.TryGetValue(clientId, out int slot))
            {
                slot = 0; // fallback
                Debug.LogWarning($"[Session] clientId={clientId} had no " +
                                 $"slot mapping — using slot 0 as fallback");
            }

            savedSlots[slot] = new PlayerSavedState
            {
                health = ph.currentHealth.Value,
                cooldowns = cds,
                hasData = true
            };

            Debug.Log($"[Session] :) SAVED slot={slot} clientId={clientId} " +
                      $"health={ph.currentHealth.Value} " +
                      $"cooldowns=[{string.Join(",", cds)}]");

            // KEY FIX — remove clientId from map AFTER saving
            // This makes the slot appear "unoccupied" for reconnect detection
            clientSlotMap.Remove(clientId);
            Debug.Log($"[Session] Removed clientId={clientId} from slot map " +
                      $"— slot {slot} now free for reconnect");

            break;
        }
    }

    private IEnumerator RestoreStateWhenReady(ulong clientId, int slot)
    {
        Debug.Log($"[Session] Restore coroutine started | " +
                  $"clientId={clientId} slot={slot}");

        float timeout = 10f;
        NetworkObject playerNetObj = null;

        while (timeout > 0f)
        {
            timeout -= Time.deltaTime;

            if (NetworkManager.Singleton.ConnectedClients
                .TryGetValue(clientId, out var client))
            {
                if (client.PlayerObject != null &&
                    client.PlayerObject.IsSpawned)
                {
                    playerNetObj = client.PlayerObject;
                    Debug.Log($"[Session] Player object found for " +
                              $"clientId={clientId}");
                    break;
                }
            }
            yield return null;
        }

        if (playerNetObj == null)
        {
            Debug.LogError($"[Session] :( TIMEOUT — player object never " +
                           $"found for clientId={clientId}");
            yield break;
        }

        // Wait for all Awake/OnNetworkSpawn to finish
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        var state = savedSlots[slot];

        // Restore health
        if (playerNetObj.TryGetComponent<PlayerHealth>(out var ph))
        {
            ph.currentHealth.Value = state.health;
            Debug.Log($"[Session] :) Restored health={state.health} " +
                      $"for clientId={clientId}");
        }
        else
        {
            Debug.LogError("[Session] :( PlayerHealth not found on player!");
        }

        // Restore cooldowns
        var abilities = playerNetObj.GetComponents<BaseAbility>();
        Debug.Log($"[Session] Restoring {abilities.Length} abilities");

        for (int i = 0;
             i < abilities.Length && i < state.cooldowns.Length;
             i++)
        {
            abilities[i].SetCooldown(state.cooldowns[i]);
            Debug.Log($"[Session] :) Restored ability[{i}] " +
                      $"cooldown={state.cooldowns[i]:F2}");
        }

        // Clean up
        savedSlots.Remove(slot);
        Debug.Log($"[Session] :) Restore complete for clientId={clientId}");
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }
}