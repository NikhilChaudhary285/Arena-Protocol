using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ScoreManager : NetworkBehaviour
{
    public NetworkVariable<int> teamScore =
        new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public TMP_Text scoreText;

    public override void OnNetworkSpawn()
    {
        teamScore.OnValueChanged += UpdateScoreUI;
        UpdateScoreUI(0, teamScore.Value);
    }

    public void AddScore(int amount)
    {
        if (!IsServer) return;
        teamScore.Value += amount;
    }

    private void UpdateScoreUI(int oldVal, int newVal)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {newVal}";
    }
}