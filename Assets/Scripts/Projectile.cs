using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float damage = 20f;
    public float lifetime = 3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Called by server right after Spawn()
    public void SetVelocity(Vector3 direction, float speed)
    {
        if (!IsServer) return;

        Vector3 velocity = direction * speed;

        // Set on server
        rb.velocity = velocity;

        // Sync velocity to all clients
        SetVelocityClientRpc(velocity);

        // Start lifetime countdown
        Invoke(nameof(DestroyProjectile), lifetime);
    }

    [ClientRpc]
    private void SetVelocityClientRpc(Vector3 velocity)
    {
        // Set velocity on each client so it moves visually
        if (rb != null)
            rb.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Don't hit other projectiles
        if (other.GetComponent<Projectile>() != null) return;

        // Don't hit players
        if (other.GetComponent<PlayerController>() != null) return;

        if (other.TryGetComponent<EnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(damage);
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        if (!IsServer) return;
        if (IsSpawned)
            GetComponent<NetworkObject>().Despawn(true);
    }
}