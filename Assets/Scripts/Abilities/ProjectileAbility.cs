using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : BaseAbility
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    protected override void Activate(PlayerController player)
    {
        // Get mouse world position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float dist))
        {
            Vector3 target = ray.GetPoint(dist);
            Vector3 dir = (target - player.transform.position).normalized;
            GameObject proj = Instantiate(projectilePrefab,
                player.transform.position + dir, Quaternion.LookRotation(dir));
            proj.GetComponent<Rigidbody>().velocity = dir * projectileSpeed;
            // Spawn on network
            proj.GetComponent<NetworkObject>().Spawn();
        }
    }
}