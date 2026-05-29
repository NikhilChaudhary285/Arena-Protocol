using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

// AbilityUI itself needed NO logic changes.
// The fix was in BaseAbility: cooldownTimer is now a NetworkVariable,
// so CooldownRemaining returns the server-authoritative value on ALL clients.
// Previously AbilityUI read a plain float that only changed on the server —
// Player B's UI always showed 0 (no cooldown overlay, no countdown text).
// Now that the NetworkVariable replicates automatically, this UI just works.
public class AbilityUI : MonoBehaviour
{
    public BaseAbility[] abilities;
    public Image[] cooldownOverlays;
    public TMP_Text[] cooldownTexts;

    private bool initialized = false;

    private void Start()
    {
        if (!initialized)
            InitializeAbilities();
    }

    void Update()
    {
        if (!initialized)
            InitializeAbilities();

        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] == null) continue;

            float remaining = abilities[i].CooldownRemaining;  // reads NetworkVariable
            float fill = abilities[i].cooldownDuration > 0
                ? remaining / abilities[i].cooldownDuration
                : 0f;

            if (cooldownOverlays != null && i < cooldownOverlays.Length && cooldownOverlays[i] != null)
                cooldownOverlays[i].fillAmount = fill;

            if (cooldownTexts != null && i < cooldownTexts.Length && cooldownTexts[i] != null)
                cooldownTexts[i].text = remaining > 0.1f ? remaining.ToString("F1") : "";
        }
    }

    private void InitializeAbilities()
    {
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.IsOwner)
            {
                abilities = player.GetComponents<BaseAbility>();
                initialized = true;
                break;
            }
        }
    }
}