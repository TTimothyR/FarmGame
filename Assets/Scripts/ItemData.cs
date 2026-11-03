using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public Sprite icon;
    public Sprite requiredSeasonIcon;
    public Color rotColor;
    public GameObject prefab;
}
