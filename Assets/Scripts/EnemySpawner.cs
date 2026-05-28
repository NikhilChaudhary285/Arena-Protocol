using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;
    public int enemiesPerWave = 3;
    public float timeBetweenWaves = 15f;
    private float waveTimer = 3f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        waveTimer = 3f;
    }

    void Update()
    {
        if (!IsServer) return;
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0)
        {
            SpawnWave();
            waveTimer = timeBetweenWaves;
        }
    }

    private void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}