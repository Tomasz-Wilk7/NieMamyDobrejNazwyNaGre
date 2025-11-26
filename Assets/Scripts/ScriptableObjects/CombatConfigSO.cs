using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatConfig", menuName = "Combat/Combat Config")]
public class CombatConfigSO : ScriptableObject
{
    [Header("Combat Stats")]
    public float AttackDamage = 10.0f;
    public float AttackRange = 2.0f;
    public float AttackCooldown = 0.5f;
    public LayerMask EnemyLayer;

    [Header("Combo Rules")]
    public int MaxComboCount = 3;
    public float ComboResetTime = 1.0f;

    [Header("Parry Rules")]
    public float ParryWindow = 0.3f;
    public float ParryCooldown = 1.0f;
    public float ParryStunDuration = 1.5f;
}
