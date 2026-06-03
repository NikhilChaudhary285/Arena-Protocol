using Unity.Netcode;
using UnityEngine;

public class EnergyOrb : NetworkBehaviour
{
    public float respawnDelay = 5f;

    // Use NetworkVariable for visibility — works even when inactive
    private NetworkVariable<bool> isVisible = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private Vector3 spawnPosition;
    private MeshRenderer meshRenderer;
    private Collider orbCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        orbCollider = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        spawnPosition = transform.position;

        // Subscribe to visibility changes
        isVisible.OnValueChanged += OnVisibilityChanged;

        // Apply initial state
        SetVisuals(isVisible.Value);
    }

    private void OnVisibilityChanged(bool oldVal, bool newVal)
    {
        SetVisuals(newVal);
    }

    private void SetVisuals(bool visible)
    {
        if (meshRenderer != null) meshRenderer.enabled = visible;
        if (orbCollider != null) orbCollider.enabled = visible;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!isVisible.Value) return; // already collected

        if (other.GetComponent<PlayerController>() != null)
        {
            // Add score
            FindObjectOfType<ScoreManager>()?.AddScore(5);

            // Hide orb via NetworkVariable — syncs to all clients
            isVisible.Value = false;

            // Schedule respawn on server
            Invoke(nameof(RespawnOrb), respawnDelay);
        }
    }

    private void RespawnOrb()
    {
        if (!IsServer) return;

        // Show orb again via NetworkVariable — syncs to all clients
        isVisible.Value = true;
    }

    public override void OnDestroy()
    {
        isVisible.OnValueChanged -= OnVisibilityChanged;
    }
}