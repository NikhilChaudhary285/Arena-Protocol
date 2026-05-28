using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public BaseAbility[] abilities;
    public Image[] cooldownOverlays;
    public TMP_Text[] cooldownTexts;

    void Update()
    {
        if (abilities == null) return;
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] == null) continue;
            float remaining = abilities[i].CooldownRemaining;
            float fill = abilities[i].cooldownDuration > 0
                ? remaining / abilities[i].cooldownDuration : 0;

            if (cooldownOverlays != null && i < cooldownOverlays.Length && cooldownOverlays[i] != null)
                cooldownOverlays[i].fillAmount = fill;

            if (cooldownTexts != null && i < cooldownTexts.Length && cooldownTexts[i] != null)
                cooldownTexts[i].text = remaining > 0.1f ? remaining.ToString("F1") : "";
        }
    }
}