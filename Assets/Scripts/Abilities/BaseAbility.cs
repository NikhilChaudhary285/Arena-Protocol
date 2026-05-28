using UnityEngine;

public abstract class BaseAbility : MonoBehaviour
{
    public string abilityName;
    public float cooldownDuration = 3f;
    protected float cooldownTimer = 0f;
    public bool IsReady => cooldownTimer <= 0f;
    public float CooldownRemaining => cooldownTimer;

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