using Unity.Netcode;
using UnityEngine;

public class EnergyOrb : NetworkBehaviour
{
    public float respawnDelay = 5f;

    [Header("Collection VFX")]
    [Tooltip("Assign a ParticleSystem child of this GameObject. " +
             "Play On Awake = false, Loop = false, Stop Action = None.")]
    public ParticleSystem collectEffect;

    [Tooltip("Match to your particle Duration + Start Lifetime. " +
             "Orb mesh is hidden for this long before the object deactivates.")]
    public float vfxDuration = 1f;

    private Vector3 spawnPosition;
    private Renderer orbRenderer;

    // FIX: server-side guard flag.
    // OnTriggerEnter runs every physics frame the collider overlaps.
    // The collider stays active on the server until SetActive(false) fires
    // on clients — which only happens after vfxDuration seconds via ClientRpc.
    // Without this flag, every frame of overlap adds score and queues another
    // Invoke(RespawnOrb), causing repeated collects and stacked respawns.
    // Setting this true on the FIRST collect blocks all subsequent enter events
    // until RespawnOrb resets it, which is the correct server-authoritative gate.
    private bool _isCollected = false;

    private void Awake()
    {
        orbRenderer = GetComponent<Renderer>();

        if (collectEffect == null)
            collectEffect = BuildDefaultEffect();
    }

    public override void OnNetworkSpawn()
    {
        spawnPosition = transform.position;
    }

    // ── Collection ──────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (_isCollected) return;                               // FIX: gate here
        if (other.GetComponent<PlayerController>() == null) return;

        _isCollected = true;                                    // lock immediately

        FindObjectOfType<ScoreManager>()?.AddScore(5);
        PlayCollectVFXClientRpc();
        Invoke(nameof(RespawnOrb), respawnDelay);
    }

    [ClientRpc]
    private void PlayCollectVFXClientRpc()
    {
        if (orbRenderer != null)
            orbRenderer.enabled = false;

        if (collectEffect != null)
        {
            collectEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            collectEffect.Play();
        }

        Invoke(nameof(DeactivateOrb), vfxDuration);
    }

    private void DeactivateOrb()
    {
        gameObject.SetActive(false);
    }

    // ── Respawn ─────────────────────────────────────────────────────────────

    private void RespawnOrb()
    {
        transform.position = spawnPosition;
        _isCollected = false;  // unlock for next collect
        ShowOrbClientRpc();
    }

    [ClientRpc]
    private void ShowOrbClientRpc()
    {
        if (collectEffect != null)
            collectEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (orbRenderer != null)
            orbRenderer.enabled = true;

        gameObject.SetActive(true);
    }

    // ── Default built-in effect (reused, never destroyed) ───────────────────
    private ParticleSystem BuildDefaultEffect()
    {
        GameObject vfxGo = new GameObject("OrbCollectVFX");
        vfxGo.transform.SetParent(transform);
        vfxGo.transform.localPosition = Vector3.zero;

        ParticleSystem ps = vfxGo.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.6f;
        main.startSpeed = 4f;
        main.startSize = 0.15f;
        main.startColor = new ParticleSystem.MinMaxGradient(
                                   new Color(1f, 0.85f, 0.1f),
                                   new Color(1f, 0.45f, 0.0f));
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.None;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        return ps;
    }
}