using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    public float maxHealth = 100f;
    public NetworkVariable<float> currentHealth =
        new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - amount);
        if (currentHealth.Value <= 0) HandleDeath();
    }

    public void Heal(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
    }

    private void HandleDeath()
    {
        // Handle player death (respawn logic)
        Debug.Log($"{gameObject.name} died!");
    }
}