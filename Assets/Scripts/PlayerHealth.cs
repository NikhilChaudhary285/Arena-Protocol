using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    public float maxHealth = 100f;

    public NetworkVariable<float> currentHealth =
        new NetworkVariable<float>(100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    // Own health bar — found at runtime via GameObject.Find
    private Slider myHealthBar;

    // Partner health bar — found at runtime via GameObject.Find
    private Slider partnerHealthBar;

    private CanvasGroup partnerHealthBarCanvasGroup;
    private CanvasGroup partnerHealthLabelCanvasGroup;

    public override void OnNetworkSpawn()
    {
        // Subscribe to health changes
        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsOwner)
        {
            // This is MY player — connect to MY health bar
            FindAndSetupMyHealthBar();
        }
        else
        {
            // This is the PARTNER player — connect to PARTNER health bar
            FindAndSetupPartnerHealthBar();
        }
    }

    private void FindAndSetupMyHealthBar()
    {
        GameObject sliderObj = GameObject.Find("HealthBar");
        if (sliderObj != null)
        {
            myHealthBar = sliderObj.GetComponent<Slider>();
            if (myHealthBar != null)
            {
                myHealthBar.minValue = 0;
                myHealthBar.maxValue = maxHealth;
                myHealthBar.value = currentHealth.Value;
                Debug.Log("[PlayerHealth] My health bar connected.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] HealthBar not found in scene!");
        }
    }

    private void FindAndSetupPartnerHealthBar()
    {
        GameObject sliderObj = GameObject.Find("PartnerHealthBar");
        GameObject partnerHealthLabel = GameObject.Find("PartnerHealthLabel");
        if (sliderObj != null && partnerHealthLabel != null)
        {
            partnerHealthBar = sliderObj.GetComponent<Slider>();
            if (partnerHealthBar != null)
            {
                partnerHealthBar.minValue = 0;
                partnerHealthBar.maxValue = maxHealth;
                partnerHealthBar.value = currentHealth.Value;

                // Get CanvasGroup from PartnerHealthBar
                partnerHealthBarCanvasGroup =
                    partnerHealthBar.GetComponent<CanvasGroup>();

                // Show PartnerHealthBar smoothly
                if (partnerHealthBarCanvasGroup != null)
                {
                    partnerHealthBarCanvasGroup.alpha = 1;
                    partnerHealthBarCanvasGroup.interactable = true;
                    partnerHealthBarCanvasGroup.blocksRaycasts = true;
                }

                // Get CanvasGroup from PartnerHealthLabel
                partnerHealthLabelCanvasGroup =
                    partnerHealthLabel.GetComponent<CanvasGroup>();
                
                // Show PartnerHealthLabel smoothly
                if (partnerHealthLabelCanvasGroup != null)
                {
                    partnerHealthLabelCanvasGroup.alpha = 1;
                    partnerHealthLabelCanvasGroup.interactable = true;
                    partnerHealthLabelCanvasGroup.blocksRaycasts = true;
                }

                Debug.Log("[PlayerHealth] PartnerHealthBar and PartnerHealthLabel connected successfully.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] PartnerHealthBar or PartnerHealthLabel not found in scene!");
        }
    }

    private void OnHealthChanged(float oldVal, float newVal)
    {
        // Update MY health bar if I own this player
        if (IsOwner && myHealthBar != null)
            myHealthBar.value = newVal;

        // Update PARTNER health bar if this is the partner
        if (!IsOwner && partnerHealthBar != null)
            partnerHealthBar.value = newVal;
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(0, currentHealth.Value - amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        currentHealth.Value = Mathf.Min(maxHealth,
            currentHealth.Value + amount);
    }

    public void Heal(float amount)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Min(maxHealth,
            currentHealth.Value + amount);
    }

    public override void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
}