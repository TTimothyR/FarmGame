using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private ItemData shovel;
    [SerializeField] private ItemData wateringCan;
    
    public ItemData HoldingItem { get; private set; }
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
        if (targetItem != null)
        {
            if (HoldingItem == targetItem)
            {
                HoldingItem = null;
                return false;
            }
            else
            {
                HoldingItem = targetItem;
                return true;
            }
        }
        return false;
    }
}
