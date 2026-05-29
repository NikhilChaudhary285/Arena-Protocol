using UnityEngine;

public class HealAbility : BaseAbility
{
    public float healAmount = 30f;
    void Awake() 
    { 
        abilityName = "Heal"; 
        //cooldownDuration = 8f; 
    }

    // FIX: Activate signature updated to match BaseAbility.
    // HealAbility does not use inputDirection at all (healing needs no aim),
    // so we simply accept and discard it. No other logic changes.
    protected override void Activate(PlayerController player, Vector3 inputDirection)
    {
        player.GetComponent<PlayerHealth>()?.HealServerRpc(healAmount);
    }
}