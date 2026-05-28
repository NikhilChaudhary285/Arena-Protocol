using UnityEngine;

public class HealAbility : BaseAbility
{
    public float healAmount = 30f;

    void Awake() { abilityName = "Heal"; cooldownDuration = 8f; }

    protected override void Activate(PlayerController player)
    {
        player.GetComponent<PlayerHealth>()?.HealServerRpc(healAmount);
    }
}