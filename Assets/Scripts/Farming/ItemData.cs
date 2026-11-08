using UnityEngine;
/// <summary>
/// A scriptable object used to construct all selectable Items from the hotbar
/// Tools like the watering can and shovel have requiredSeasonIcon, rotColor and prefab set to null.
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public Sprite icon;
    public Sprite requiredSeasonIcon;
    public Color rotColor;
    public GameObject prefab;
}
