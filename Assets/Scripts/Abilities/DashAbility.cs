using UnityEngine;

public class DashAbility : BaseAbility
{
    public float dashDistance = 3f;

    protected override void Activate(PlayerController player)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        if (dir == Vector3.zero) dir = player.transform.forward;
        player.transform.position += dir * dashDistance;
    }
}