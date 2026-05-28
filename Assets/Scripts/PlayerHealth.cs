using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public float maxHealth = 100f;

    public NetworkVariable<float> currentHealth =
        new NetworkVariable<float>(100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private Slider healthBarSlider;

    public override void OnNetworkSpawn()
    {
        // Only the LOCAL player (owner) should update this UI
        if (IsOwner)
        {
            // Find the HealthBar slider in the scene at runtime
            GameObject sliderObj = GameObject.Find("HealthBar");
            if (sliderObj != null)
                healthBarSlider = sliderObj.GetComponent<Slider>();

            // Set initial value
            if (healthBarSlider != null)
            {
                healthBarSlider.minValue = 0;
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth.Value;
            }
        }

        // Listen for changes on all clients
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float oldVal, float newVal)
    {
        // Only update UI for the player that owns this character
        if (IsOwner && healthBarSlider != null)
            healthBarSlider.value = newVal;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
    }

    public void Heal(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
    }

    public override void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
}