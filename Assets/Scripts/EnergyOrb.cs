using Unity.Netcode;
using UnityEngine;

public class EnergyOrb : NetworkBehaviour
{
    public float respawnDelay = 5f;
    private Vector3 spawnPosition;

    public override void OnNetworkSpawn()
    {
        spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.GetComponent<PlayerController>() != null)
            CollectOrbClientRpc();
    }

    [ClientRpc]
    private void CollectOrbClientRpc()
    {
        gameObject.SetActive(false);
        if (IsServer)
        {
            FindObjectOfType<ScoreManager>()?.AddScore(5);
            Invoke(nameof(RespawnOrb), respawnDelay);
        }
    }

    private void RespawnOrb()
    {
        transform.position = spawnPosition;
        ShowOrbClientRpc();
    }

    [ClientRpc]
    private void ShowOrbClientRpc()
    {
        gameObject.SetActive(true);
    }
}