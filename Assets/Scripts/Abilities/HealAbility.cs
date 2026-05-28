using UnityEngine;

public class HealAbility : BaseAbility
{
    public float healAmount = 30f;

    protected override void Activate(PlayerController player)
    {
        player.GetComponent<PlayerHealth>().Heal(healAmount);
    }
}