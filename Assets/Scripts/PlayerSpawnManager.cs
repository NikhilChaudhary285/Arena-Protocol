using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    // Just holds the spawn point references
    // Actual spawning is handled inside PlayerController
    public Transform[] spawnPoints;
}