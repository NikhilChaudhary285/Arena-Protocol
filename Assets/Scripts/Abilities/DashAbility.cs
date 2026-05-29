using UnityEngine;

public class DashAbility : BaseAbility
{
    public float dashDistance = 3f;

    void Awake()
    {
        abilityName = "Dash";
        //cooldownDuration = 4f;
    }

    // FIX: inputDirection is passed IN from the client's HandleAbilities()
    // via ServerRpc. Previously this method called Input.GetAxisRaw() directly,
    // which always returns 0 on the server — so Player B always dashed forward
    // and the direction felt broken. Now direction is captured on the owning
    // client and sent up with the ability index.
    protected override void Activate(PlayerController player, Vector3 inputDirection)
    {
        Vector3 dir = inputDirection.sqrMagnitude > 0.01f
            ? inputDirection
            : player.transform.forward;   // fallback if no key held

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.Move(dir * dashDistance);
    }
}