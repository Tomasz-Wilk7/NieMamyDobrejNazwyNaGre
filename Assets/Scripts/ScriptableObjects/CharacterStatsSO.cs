using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterConfig", menuName = "Combat/Character Profile")]
public class CharacterStatsSO : ScriptableObject
{
    [Tooltip("The base maximum health for this entity.")]
    public float MaxHealth = 100f;
}
