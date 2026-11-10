using TMPro;
using UnityEngine;

public class HealthHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private TMP_Text healthText;

    [Header("Vida baja")]
    [Range(0f, 1f)]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowHealthColor = Color.red;

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged += UpdateText;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateText;
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("HealthHUD: falta asignar el Health del jugador.", this);
            enabled = false;
            return;
        }

        UpdateText(playerHealth.Current, playerHealth.Max);
    }

    private void UpdateText(float current, float max)
    {
        healthText.text = $"Vida: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        bool isLow = max > 0f && current / max <= lowHealthThreshold;
        healthText.color = isLow ? lowHealthColor : normalColor;
    }
}