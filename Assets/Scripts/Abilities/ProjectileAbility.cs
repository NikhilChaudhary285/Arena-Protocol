using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : BaseAbility
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;

    void Awake()
    {
        abilityName = "Shoot";
        //cooldownDuration = 1f;
    }

    // FIX 1: Signature updated to accept inputDirection from BaseAbility.
    //
    // FIX 2: Camera.main and Input.mousePosition were called here, but
    // Activate() runs on the SERVER (called from ActivateAbilityServerRpc).
    // On the server Camera.main is null → NullReferenceException for Player B,
    // and Input.mousePosition is always (0,0) so direction was always wrong.
    //
    // Solution: the aim direction is now computed CLIENT-SIDE in
    // PlayerController.HandleAbilities() and passed through the ServerRpc
    // as inputDirection — exactly the same pattern as DashAbility.
    // ProjectileAbility just uses the direction it receives.
    protected override void Activate(PlayerController player, Vector3 inputDirection)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("ProjectileAbility: projectilePrefab is null!");
            return;
        }

        // inputDirection already computed on the owning client and sent via
        // ServerRpc — no Camera or Input calls needed here.
        Vector3 dir = inputDirection.sqrMagnitude > 0.01f
            ? inputDirection
            : player.transform.forward;   // fallback if cursor exactly on player

        Vector3 spawnPos = player.transform.position + dir * 0.8f;
        SpawnProjectileServerRpc(spawnPos, dir);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectileServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (!IsServer) return;

        GameObject proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction));

        NetworkObject netObj = proj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Projectile prefab missing NetworkObject component!");
            Destroy(proj);
            return;
        }

        netObj.Spawn(true);

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.SetVelocity(direction, projectileSpeed);
    }
}