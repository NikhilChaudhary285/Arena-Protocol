using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private CharacterController controller;
    private float gravity = -9.81f;
    private Vector3 velocity;

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
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);

        GetComponent<Renderer>().material.color =
            OwnerClientId == 0 ? Color.blue : Color.green;

        if (IsServer)
            StartCoroutine(MoveToSpawnPoint());

        Debug.Log($"Player spawned | Owner: {OwnerClientId} | " +
                  $"LocalClient: {NetworkManager.Singleton.LocalClientId} | " +
                  $"IsOwner: {IsOwner}");
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
        // Send movement to server every frame
        if (dir != Vector3.zero)
            MoveServerRpc(dir);
    }

    // SERVER handles all movement — no FixedUpdate on client
    // This prevents dash conflict completely
    [ServerRpc]
    private void MoveServerRpc(Vector3 direction)
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        controller.Move(direction * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleAbilities()
    {
        if (abilities == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (Input.GetKeyDown(KeyCode.Q) && abilities.Length > 0)
            abilities[0]?.TryActivate(this, inputDir);

        if (Input.GetKeyDown(KeyCode.E) && abilities.Length > 1)
            abilities[1]?.TryActivate(this, inputDir);

        if (Input.GetKeyDown(KeyCode.R) && abilities.Length > 2)
            abilities[2]?.TryActivate(this, inputDir);
    }

    private System.Collections.IEnumerator MoveToSpawnPoint()
    {
        yield return null;
        yield return null;

        PlayerSpawnManager spawnManager =
            FindObjectOfType<PlayerSpawnManager>();

        if (spawnManager == null) yield break;
        if (spawnManager.spawnPoints == null) yield break;
        if (spawnManager.spawnPoints.Length == 0) yield break;

        int index = (int)(OwnerClientId
            % (ulong)spawnManager.spawnPoints.Length);
        Vector3 targetPos = spawnManager.spawnPoints[index].position;

        transform.position = targetPos;

        GetComponent<NetworkTransform>()?.Teleport(
            targetPos,
            transform.rotation,
            transform.localScale);

        Debug.Log($"[Player] OwnerClientId {OwnerClientId} " +
                  $"teleported to {targetPos}");
    }
}