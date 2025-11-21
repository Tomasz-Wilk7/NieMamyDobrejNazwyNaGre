using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileConfigSO", menuName = "Combat/Projectile")]
public class ProjectileConfigSO : ScriptableObject
{
    public string poolTag;
    public float speed;
    public float damage;
    public float lifetime;
}
