using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyHealth : NetworkBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    public override void OnNetworkSpawn()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // Add score before despawning
        //FindObjectOfType<ScoreManager>()?.AddScore(10);
        GetComponent<NetworkObject>().Despawn();
    }
}