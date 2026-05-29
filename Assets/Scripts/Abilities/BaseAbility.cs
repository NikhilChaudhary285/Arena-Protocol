using Unity.Netcode;
using UnityEngine;

// FIX: cooldownTimer is now a NetworkVariable so ALL clients see the real value.
// Previously it was a plain float that only ticked on the server — Player B's
// client had its own copy stuck at 0, so the UI never showed a cooldown and
// IsReady was always true on the client side.
public abstract class BaseAbility : NetworkBehaviour
{
    public string abilityName = "Ability";
    public float cooldownDuration = 3f;

    // ── Networked cooldown ──────────────────────────────────────────────────
    // Read permission: Everyone (clients need to read it for UI + IsReady check)
    // Write permission: Server only (server is authoritative — only it deducts time)
    private NetworkVariable<float> _cooldownTimer = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Public accessors used by AbilityUI and TryActivate
    public bool IsReady => _cooldownTimer.Value <= 0f;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer.Value);

    // Read-only shim — keeps any existing read references compiling.
    // Do NOT assign to this; use SetCooldown() instead.
    public float cooldownTimer => _cooldownTimer.Value;

    // Called by SessionManager on reconnect to restore saved cooldown state.
    // Must only be called on the server (NetworkVariable write permission = Server).
    public void SetCooldown(float value)
    {
        if (!IsServer)
        {
            Debug.LogWarning("SetCooldown called on non-server — ignored.");
            return;
        }
        _cooldownTimer.Value = Mathf.Max(0f, value);
    }

    protected virtual void Update()
    {
        // Only the server ticks the timer down — it owns the NetworkVariable.
        if (!IsServer) return;
        if (_cooldownTimer.Value > 0f)
            _cooldownTimer.Value -= Time.deltaTime;
    }

    // Called by PlayerController via ServerRpc — always runs on server
    public void TryActivate(PlayerController player, Vector3 inputDirection)
    {
        if (!IsReady) return;
        Activate(player, inputDirection);   // pass direction from client
        _cooldownTimer.Value = cooldownDuration;
    }

    // Subclasses implement combat logic; receive pre-computed direction
    // so they never need to call Input.GetAxisRaw (which returns 0 on server)
    protected abstract void Activate(PlayerController player, Vector3 inputDirection);
}