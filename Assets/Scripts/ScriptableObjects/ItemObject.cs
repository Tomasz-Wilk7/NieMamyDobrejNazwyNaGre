using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Scriptable Objects/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName = "New Item";

    [TextArea(3,10)]
    public string Description = "Item Description";

    public int goldValue = 10;
    public Sprite Icon;

    public float DamageModifier = 1.0f;
    public float Weight = 1.0f;
    public bool IsConsumable = false;
}
