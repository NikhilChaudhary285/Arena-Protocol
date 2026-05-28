using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private CharacterController controller;
    [SerializeField] private BaseAbility[] abilities;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        abilities = GetComponents<BaseAbility>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
        }
        GetComponent<Renderer>().material.color =
            OwnerClientId == 0 ? Color.blue : Color.green;
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleMovement();
        HandleAbilities();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        controller.Move(dir * moveSpeed * Time.deltaTime);
    }

    private void HandleAbilities()
    {
        if (abilities == null) return;
        if (Input.GetKeyDown(KeyCode.Q) && abilities.Length > 0) abilities[0]?.TryActivate(this); // Dash
        if (Input.GetKeyDown(KeyCode.E) && abilities.Length > 1) abilities[1]?.TryActivate(this); // Projectile
        if (Input.GetKeyDown(KeyCode.R) && abilities.Length > 2) abilities[2]?.TryActivate(this); // Heal
    }
}