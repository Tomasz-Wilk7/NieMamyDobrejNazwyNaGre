using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The image component representing the health fill.")]
    [SerializeField] private Image healthFillImage;
    
    [Header("Setup")]
    [Tooltip("The AttackScript component on this GameObject or a parent/child.")]
    [SerializeField] private AttackScript targetAttackScript;
    [Tooltip("Offset position of the health bar in world space.")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    
    private Transform _targetTransform;

    private void Awake()
    {
        if (_targetTransform == null)
        {
            targetAttackScript = GetComponentInParent<AttackScript>();
        }
        
        _targetTransform = targetAttackScript.transform;
    }

    private void OnEnable()
    {
        if (targetAttackScript != null)
        {
            targetAttackScript.OnHealthChanged += UpdateHealthBar;
            targetAttackScript.OnDeath += OnTargetDeath;
            
            UpdateHealthBar(targetAttackScript.CurrentHealth);
        }
    }

    private void OnDisable()
    {
        if (targetAttackScript != null)
        {
            targetAttackScript.OnHealthChanged -= UpdateHealthBar;
            targetAttackScript.OnDeath -= OnTargetDeath;
        }
    }

    private void UpdateHealthBar(float newHealth)
    {
        if (healthFillImage != null) return;
        float healthRatio = newHealth / targetAttackScript.MaxHealth;
        
        healthFillImage.fillAmount = healthRatio;
        
    }

    private void OnTargetDeath()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_targetTransform != null)
        {
            transform.position = _targetTransform.position + offset;

            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
