using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    public PlayerController player;
    public Image dashBar;
    public TMP_Text statusText;

    private void Update()
    {
        if (player == null)
        {
            if (dashBar != null)
                dashBar.fillAmount = 0f;
            if (statusText != null)
                statusText.text = "DASH UNAVAILABLE";
            return;
        }

        float cooldown = Mathf.Max(0f, player.dashCooldown);
        float remaining = Mathf.Max(0f, player.dashCooldownTimer);
        bool ready = cooldown <= 0f || remaining <= 0f;

        if (dashBar != null)
            dashBar.fillAmount = cooldown <= 0f ? 1f : Mathf.Clamp01(1f - remaining / cooldown);

        if (statusText != null)
        {
            statusText.text = ready ? "DASH READY" : "RECHARGING  " + remaining.ToString("0.0") + "s";
            statusText.color = ready ? SnowUIFactory.Cyan : SnowUIFactory.MutedIce;
        }
    }
}
