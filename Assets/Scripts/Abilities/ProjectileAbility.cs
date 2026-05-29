using System;
using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : BaseAbility
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;

    void Awake()
    {
        abilityName = "Shoot";
        cooldownDuration = 1f;
    }

    protected override void Activate(PlayerController player)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("ProjectileAbility: projectilePrefab is null!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 target = ray.GetPoint(dist);
            Vector3 dir = (target - player.transform.position);
            dir.y = 0;
            dir.Normalize();

            if (dir == Vector3.zero)
                dir = player.transform.forward;

            Vector3 spawnPos = player.transform.position + dir * 0.8f;

            // Tell the SERVER to spawn the projectile
            SpawnProjectileServerRpc(spawnPos, dir);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectileServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        // Double check we are on server
        if (!IsServer) return;

        // Instantiate on server
        GameObject proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction));

        // Get NetworkObject BEFORE spawning
        NetworkObject netObj = proj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("Projectile prefab missing NetworkObject component!");
            Destroy(proj);
            return;
        }

        // Spawn on network FIRST
        netObj.Spawn(true);

        // Set velocity AFTER spawn via the Projectile script
        // NOT directly here — let the Projectile handle it
        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.SetVelocity(direction, projectileSpeed);
    }
}