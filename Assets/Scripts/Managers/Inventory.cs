using UnityEngine;
/// <summary>
/// Helper singleton to manage the item the player currently has equipped
/// Other initialized items are used by other scripts to see if the holding item is the right one for its behaviour.
/// </summary>
public class Inventory : MonoBehaviour
{
    [SerializeField] private ItemData radishSeed;
    [SerializeField] private ItemData pumpkinSeed;
    [SerializeField] private ItemData shovel;
    [SerializeField] private ItemData wateringCan;
    
    public ItemData HoldingItem { get; private set; }
    public ItemData RadishSeed => radishSeed;
    public ItemData PumpkinSeed => pumpkinSeed;
    public ItemData Shovel => shovel;
    public ItemData WateringCan => wateringCan;
    
    public static Inventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool ToggleItem(ItemData targetItem)
    {
        if (targetItem == null)
        {
            return false;
        }

        if (HoldingItem == targetItem)
        {
            HoldingItem = null;
            return false;
        }

        HoldingItem = targetItem;
        return true;
    }
}
