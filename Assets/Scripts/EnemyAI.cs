using Unity.Netcode;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("Stats")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    private State currentState = State.Patrol;
    private Transform target;
    private float attackTimer;
    private Vector3 patrolTarget;
    private float patrolTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (!IsServer) return; // All AI runs on server only
        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
        }
    }

    private void DoPatrol()
    {
        patrolTimer -= Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position,
            patrolTarget, moveSpeed * 0.5f * Time.deltaTime);

        if (patrolTimer <= 0 || Vector3.Distance(transform.position, patrolTarget) < 0.5f)
            SetNewPatrolTarget();

        // Check for players
        target = FindClosestPlayer();
        if (target != null && Vector3.Distance(transform.position, target.position) < detectionRange)
            currentState = State.Chase;
    }

    private void DoChase()
    {
        if (target == null) { currentState = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > detectionRange) { currentState = State.Patrol; target = null; return; }
        if (dist <= attackRange) { currentState = State.Attack; return; }

        transform.position = Vector3.MoveTowards(transform.position,
            target.position, moveSpeed * Time.deltaTime);
    }

    private void DoAttack()
    {
        if (target == null) { currentState = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > attackRange) { currentState = State.Chase; return; }

        if (attackTimer <= 0)
        {
            target.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            attackTimer = attackCooldown;
        }
    }

    private Transform FindClosestPlayer()
    {
        // Find all NetworkObjects that are players
        float minDist = float.MaxValue;
        Transform closest = null;
        foreach (var pc in FindObjectsOfType<PlayerController>())
        {
            float d = Vector3.Distance(transform.position, pc.transform.position);
            if (d < minDist) { minDist = d; closest = pc.transform; }
        }
        return closest;
    }

    private void SetNewPatrolTarget()
    {
        patrolTarget = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
        patrolTimer = Random.Range(3f, 6f);
    }
}