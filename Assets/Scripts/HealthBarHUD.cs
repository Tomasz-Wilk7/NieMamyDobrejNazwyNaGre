using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarHUD : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("The Image component that represents the fill amount of the health bar.")]
    [SerializeField] private Image healthFillImage;
    
    [Tooltip("The Text component to show health value (e.g., 80/100).")]
    [SerializeField] private Text healthText; 

    [Header("Setup")]
    [Tooltip("Assign the Player's AttackScript component here.")]
    [SerializeField] private AttackScript playerAttackScript;

    private void Start()
    {
        if (playerAttackScript != null)
        {
            UpdateHealthBar(playerAttackScript.CurrentHealth);
        }
        
    }
    private void OnEnable()
    {
        if (playerAttackScript != null)
        {
            playerAttackScript.OnHealthChanged += UpdateHealthBar;
            
            UpdateHealthBar(playerAttackScript.CurrentHealth);
        }
    }

    private void OnDisable()
    {
        if (playerAttackScript != null)
        {
            playerAttackScript.OnHealthChanged -= UpdateHealthBar;
        }
    }
    
    private void UpdateHealthBar(float newHealth)
    {
        if (healthFillImage == null) return;
        float maxHealth = playerAttackScript.MaxHealth;
        healthFillImage.fillAmount = newHealth / maxHealth;
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(newHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }
    
}
