using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class NetworkObjectPool : MonoBehaviour
{
    public static NetworkObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolEntry
    {
        public GameObject prefab;
        public int initialSize = 10;
    }

    public PoolEntry[] pools;

    private Dictionary<GameObject, Queue<NetworkObject>> poolDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Only server creates pool objects
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var entry in pools)
        {
            var queue = new Queue<NetworkObject>();

            for (int i = 0; i < entry.initialSize; i++)
            {
                var obj = CreateNew(entry.prefab);
                queue.Enqueue(obj);
            }

            poolDict[entry.prefab] = queue;
        }
    }

    private NetworkObject CreateNew(GameObject prefab)
    {
        var go = Instantiate(prefab);
        var net = go.GetComponent<NetworkObject>();
        net.Spawn(true);
        go.SetActive(false);
        return net;
    }

    // Get from pool — call on server only
    public NetworkObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!poolDict.ContainsKey(prefab))
        {
            Debug.LogWarning($"[Pool] No pool for {prefab.name} — creating new");
            var newNet = CreateNew(prefab);
            newNet.gameObject.SetActive(true);
            newNet.transform.SetPositionAndRotation(pos, rot);
            return newNet;
        }

        var queue = poolDict[prefab];

        NetworkObject netObj;
        if (queue.Count > 0)
        {
            netObj = queue.Dequeue();
        }
        else
        {
            netObj = CreateNew(prefab);
        }

        netObj.transform.SetPositionAndRotation(pos, rot);
        netObj.gameObject.SetActive(true);
        return netObj;
    }

    // Return to pool — call on server only
    public void Return(GameObject prefab, NetworkObject netObj)
    {
        netObj.gameObject.SetActive(false);

        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<NetworkObject>();

        poolDict[prefab].Enqueue(netObj);
    }
}