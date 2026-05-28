using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public Vector3 offset = new Vector3(0, 15, 0);

    public void SetTarget(Transform t) => target = t;

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }
}