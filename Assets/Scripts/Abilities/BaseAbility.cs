using Unity.Netcode;
using UnityEngine;

public abstract class BaseAbility : NetworkBehaviour
{
    public string abilityName = "Ability";
    public float cooldownDuration = 3f;
    public float cooldownTimer = 0f;
    public bool IsReady => cooldownTimer <= 0f;
    public float CooldownRemaining => Mathf.Max(0, cooldownTimer);

    protected virtual void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void TryActivate(PlayerController player)
    {
        if (!IsReady) return;
        Activate(player);
        cooldownTimer = cooldownDuration;
    }

    protected abstract void Activate(PlayerController player);
}