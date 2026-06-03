using Unity.Netcode;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    public enum AIState { Patrol, Chase, Attack }

    [Header("Detection")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    private AIState state = AIState.Patrol;
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
        if (!IsServer) return;
        attackTimer -= Time.deltaTime;
        switch (state)
        {
            case AIState.Patrol: DoPatrol(); break;
            case AIState.Chase: DoChase(); break;
            case AIState.Attack: DoAttack(); break;
        }
    }

    private void DoPatrol()
    {
        patrolTimer -= Time.deltaTime;
        transform.position = Vector3.MoveTowards(
            transform.position, patrolTarget, moveSpeed * 0.5f * Time.deltaTime);
        if (patrolTimer <= 0 ||
            Vector3.Distance(transform.position, patrolTarget) < 0.3f)
            SetNewPatrolTarget();

        Transform closest = FindClosestPlayer();
        if (closest != null &&
            Vector3.Distance(transform.position, closest.position) < detectionRange)
        {
            target = closest;
            state = AIState.Chase;
        }
    }

    private void DoChase()
    {
        // Re-evaluate closest player every frame — no target lock
        target = FindClosestPlayer();

        if (target == null) { state = AIState.Patrol; return; }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            state = AIState.Patrol;
            target = null;
            return;
        }

        if (dist <= attackRange) { state = AIState.Attack; return; }

        transform.position = Vector3.MoveTowards(
            transform.position, target.position,
            moveSpeed * Time.deltaTime);
    }

    private void DoAttack()
    {
        // Re-evaluate closest player every frame here too
        target = FindClosestPlayer();

        if (target == null) { state = AIState.Patrol; return; }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > attackRange) { state = AIState.Chase; return; }

        if (attackTimer <= 0)
        {
            target.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
            attackTimer = attackCooldown;
        }
    }

    private Transform FindClosestPlayer()
    {
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
        patrolTarget = new Vector3(
            Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
        patrolTimer = Random.Range(3f, 6f);
    }
}