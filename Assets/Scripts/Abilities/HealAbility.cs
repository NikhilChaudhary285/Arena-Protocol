using UnityEngine;

public class HealAbility : BaseAbility
{
    public float healAmount = 30f;

    void Awake()
    {
        abilityName = "Heal";
        //cooldownDuration = 8f;
    }

    protected override void Activate(PlayerController player,
                                     Vector3 inputDirection)
    {
        // ONE call only — directly to server via PlayerHealth
        // Previously was calling both HealServerRpc AND Heal() — redundant
        player.GetComponent<PlayerHealth>()?.HealServerRpc(healAmount);
    }
}