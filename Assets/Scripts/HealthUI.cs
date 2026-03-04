using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    void Update()
    {
        if (GameManager.Instance != null && healthText != null)
            healthText.text = $"Health: {GameManager.Instance.playerHealth}/{GameManager.Instance.playerMaxHealth}";
    }
}
