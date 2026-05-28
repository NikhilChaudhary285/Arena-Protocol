using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;

    private BaseAbility[] abilities;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Start()
    {
        abilities = GetComponents<BaseAbility>();
    }

    void Update()
    {
        if (!IsOwner) return;

        // Process player movement input and networked movement
        HandleMovement();

        // Process player combat abilities and input actions
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
        if (Input.GetKeyDown(KeyCode.Q)) abilities[0]?.TryActivate(this);
        if (Input.GetKeyDown(KeyCode.E)) abilities[1]?.TryActivate(this);
        if (Input.GetKeyDown(KeyCode.R)) abilities[2]?.TryActivate(this);
    }
}