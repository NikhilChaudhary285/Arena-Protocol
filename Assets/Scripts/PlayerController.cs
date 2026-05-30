using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 moveDirection;
    private Vector3 velocity;
    public float gravity = -9.81f;
    private CharacterController controller;

    [SerializeField] private BaseAbility[] abilities;

    public T GetAbility<T>() where T : BaseAbility
    {
        if (abilities == null || abilities.Length == 0) return null;
        foreach (var ability in abilities)
            if (ability is T typed) return typed;
        return null;
    }

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

        Debug.Log($"Player spawned | Owner: {OwnerClientId} | " +
                  $"LocalClient: {NetworkManager.Singleton.LocalClientId} | IsOwner: {IsOwner}");

        if (IsServer)
            StartCoroutine(MoveToSpawnPoint());
    }

    private IEnumerator MoveToSpawnPoint()
    {
        yield return null;
        yield return null;

        PlayerSpawnManager spawnManager = FindObjectOfType<PlayerSpawnManager>();
        if (spawnManager == null || spawnManager.spawnPoints == null ||
            spawnManager.spawnPoints.Length == 0) yield break;

        int index = (int)(OwnerClientId % (ulong)spawnManager.spawnPoints.Length);
        Vector3 targetPos = spawnManager.spawnPoints[index].position;

        transform.position = targetPos;
        GetComponent<NetworkTransform>()?.Teleport(targetPos, transform.rotation, transform.localScale);

        Debug.Log($"[Player] OwnerClientId {OwnerClientId} teleported to {targetPos}");
    }

    void Update()
    {
        if (!IsOwner) return;

        // Reading input in Update — stays responsive
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(h, 0, v).normalized;

        HandleAbilities();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Move locally on the owner client in FixedUpdate
        // FixedUpdate runs at fixed 0.02s regardless of frame rate
        // This makes Editor and Build feel IDENTICAL
        controller.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Apply gravity locally too
        velocity.y += gravity * Time.fixedDeltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);

        // Tell server the new position after moving
        // Server just accepts and replicates — no movement logic on server
        SyncPositionServerRpc(transform.position);
    }

    [ServerRpc]
    private void SyncPositionServerRpc(Vector3 newPosition)
    {
        // Server accepts client position and applies it
        // NetworkTransform then replicates to all other clients
        transform.position = newPosition;
    }

    private void HandleAbilities()
    {
        if (abilities == null) return;

        // --- Movement direction (WASD) — used by Dash ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        // --- Aim direction (mouse) — used by Projectile ---
        // Computed here on the OWNING CLIENT where Camera.main exists and
        // Input.mousePosition is valid. Both would be null/zero on the server.
        Vector3 aimDir = GetMouseAimDirection();

        if (Input.GetKeyDown(KeyCode.Q))
            ActivateAbilityServerRpc(0, moveDir);   // Dash uses movement dir

        if (Input.GetKeyDown(KeyCode.E))
            ActivateAbilityServerRpc(1, aimDir);    // Projectile uses aim dir

        if (Input.GetKeyDown(KeyCode.R))
            ActivateAbilityServerRpc(2, Vector3.zero); // Heal needs no direction
    }

    // Raycasts from the camera through the mouse cursor onto the world XZ plane.
    // Returns a normalised horizontal direction vector, or Vector3.zero on failure.
    private Vector3 GetMouseAimDirection()
    {
        if (Camera.main == null) return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 target = ray.GetPoint(dist);
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                return dir.normalized;
        }
        return Vector3.zero;
    }

    // ServerRpc receives the pre-computed direction so no ability ever needs
    // to call Input or Camera on the server side.
    [ServerRpc]
    private void ActivateAbilityServerRpc(int abilityIndex, Vector3 inputDirection)
    {
        if (abilities == null) return;
        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return;
        abilities[abilityIndex]?.TryActivate(this, inputDirection);
    }
}