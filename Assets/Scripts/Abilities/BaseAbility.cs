using Unity.Netcode;
using UnityEngine;

public abstract class BaseAbility : NetworkBehaviour
{
    public string abilityName = "Ability";
    public float cooldownDuration = 3f;

    // ── Networked cooldown ──────────────────────────────────────────────────
    // Read permission : Everyone (clients need it for UI + IsReady check)
    // Write permission: Server only (server is authoritative)
    private NetworkVariable<float> _cooldownTimer = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ✅ KEPT: Public accessors — nothing changed here
    public bool IsReady => _cooldownTimer.Value <= 0f;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer.Value);

    // ✅ KEPT: Read-only shim — keeps existing read references compiling
    // SessionManager reads this on disconnect — that still works fine
    public float cooldownTimer => _cooldownTimer.Value;

    // ✅ FIX: SetCooldown guard changed from IsServer to IsSpawned + IsServer
    // Previously if NetworkObject wasn't fully spawned yet IsServer could
    // return false even on the host — causing the restore to silently fail
    public void SetCooldown(float value)
    {
        if (!IsSpawned)
        {
            Debug.LogWarning($"[BaseAbility] SetCooldown called before " +
                             $"NetworkSpawn on {gameObject.name} — ignored.");
            return;
        }

        if (!IsServer)
        {
            Debug.LogWarning($"[BaseAbility] SetCooldown called on " +
                             $"non-server for {gameObject.name} — ignored.");
            return;
        }

        _cooldownTimer.Value = Mathf.Max(0f, value);
    }

    // ✅ KEPT: Server-only tick — unchanged
    protected virtual void Update()
    {
        if (!IsServer) return;
        if (_cooldownTimer.Value > 0f)
            _cooldownTimer.Value -= Time.deltaTime;
    }

    // ✅ KEPT: TryActivate with inputDirection — unchanged
    public void TryActivate(PlayerController player, Vector3 inputDirection)
    {
        if (!IsReady) return;
        Activate(player, inputDirection);
        _cooldownTimer.Value = cooldownDuration;
    }

    // ✅ KEPT: Abstract method with inputDirection — unchanged
    protected abstract void Activate(PlayerController player,
                                     Vector3 inputDirection);
}