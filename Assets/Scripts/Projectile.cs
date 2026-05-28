using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float damage = 20f;
    public float lifetime = 3f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Invoke(nameof(DestroyProjectile), lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.TryGetComponent<EnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(damage);
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        if (IsSpawned)
            GetComponent<NetworkObject>().Despawn(true);
    }
}