using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : BaseAbility
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;

    void Awake() { abilityName = "Shoot"; cooldownDuration = 1f; }

    protected override void Activate(PlayerController player)
    {
        if (projectilePrefab == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float dist))
        {
            Vector3 target = ray.GetPoint(dist);
            Vector3 dir = (target - player.transform.position).normalized;
            dir.y = 0;
            if (dir == Vector3.zero) dir = player.transform.forward;
            SpawnProjectileServerRpc(player.transform.position + dir * 0.6f, dir);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnProjectileServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        proj.GetComponent<NetworkObject>().Spawn(true);
        proj.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;
    }
}