using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public ItemData HoldingItem { get; private set; }
    
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
                HUD.Instance.ShowNotification($"Unequipped {targetItem.name}.", Color.darkRed);
                return false;
            }
            else
            {
                HoldingItem = targetItem;
                HUD.Instance.ShowNotification($"Equipped {targetItem.name}.", Color.forestGreen);
                return true;
            }
        }
        return false;
    }
}
