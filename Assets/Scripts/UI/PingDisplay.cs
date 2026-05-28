using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PingDisplay : MonoBehaviour
{
    public TMP_Text pingText;
    private float updateTimer;
    private float smoothedPing;

    void Update()
    {
        updateTimer -= Time.deltaTime;
        if (updateTimer > 0) return;
        updateTimer = 0.5f;

        if (pingText == null) return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
        {
            pingText.text = "Ping: -- ms";
            return;
        }

        // Use Unity Transport's RTT if available
        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport
            as Unity.Netcode.Transports.UTP.UnityTransport;
        if (transport != null)
        {
            // RTT in ms (divide by 2 for one-way ping approximation)
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            var rtt = transport.GetCurrentRtt(clientId);
            smoothedPing = Mathf.Lerp(smoothedPing, rtt, 0.3f);
            pingText.text = $"Ping: {Mathf.RoundToInt(smoothedPing / 2)} ms";
        }
        else
        {
            pingText.text = "Ping: ~ms";
        }
    }
}