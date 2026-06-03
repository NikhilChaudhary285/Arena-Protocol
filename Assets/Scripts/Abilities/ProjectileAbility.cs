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

    protected override void Activate(PlayerController player,
                                     Vector3 inputDirection)
    {
        if (projectilePrefab == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 target = ray.GetPoint(dist);
            Vector3 dir = (target - player.transform.position);
            dir.y = 0;
            dir.Normalize();
            if (dir == Vector3.zero) dir = player.transform.forward;

            Vector3 spawnPos = player.transform.position + dir * 0.8f;

            // ONE ServerRpc call only — server handles everything
            SpawnProjectileServerRpc(spawnPos, dir);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectileServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (!IsServer) return;

        // Use pool instead of Instantiate
        NetworkObject netObj = NetworkObjectPool.Instance.Get(
            projectilePrefab,
            spawnPos,
            Quaternion.LookRotation(direction));

        Projectile projScript = netObj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.SetVelocity(direction, projectileSpeed);
    }
}