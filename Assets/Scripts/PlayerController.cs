using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private Vector3 velocity;
    public float gravity = -9.81f;

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
        Debug.Log($"Player spawned | Owner: {OwnerClientId} | LocalClient: {NetworkManager.Singleton.LocalClientId} | IsOwner: {IsOwner}");
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

        // Send movement input to server
        MoveServerRpc(dir);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 direction)
    {
        // Horizontal movement
        controller.Move(direction * moveSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAbilities()
    {
        if (abilities == null) return;
        if (Input.GetKeyDown(KeyCode.Q) && abilities.Length > 0) abilities[0]?.TryActivate(this); // Dash
        if (Input.GetKeyDown(KeyCode.E) && abilities.Length > 1) abilities[1]?.TryActivate(this); // Projectile
        if (Input.GetKeyDown(KeyCode.R) && abilities.Length > 2) abilities[2]?.TryActivate(this); // Heal
    }
}