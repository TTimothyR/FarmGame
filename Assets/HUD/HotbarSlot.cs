using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI keybindText;
    [SerializeField] private Button button;
    
    public ItemData AssignedItem { get; private set; }

    private Action<ItemData, HotbarSlot> onSlotClicked;

    private void Awake()
    {
        button.onClick.AddListener(() => onSlotClicked?.Invoke(AssignedItem, this));
    }

    public void Setup(ItemData item, int keybindNumber, Action<ItemData, HotbarSlot> clickCallback)
    {
        AssignedItem = item;
        onSlotClicked = clickCallback;
        keybindText.text = keybindNumber.ToString();

        if (item != null)
        {
            iconImage.sprite = item.icon;
        }
        else
        {
            iconImage.enabled = false;
        }
    }
}
