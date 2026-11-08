using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// The hotbar slot object that sets the right icon, keybind and interact callback.
/// </summary>
public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image seasonImage;
    [SerializeField] private TextMeshProUGUI keybindText;
    [SerializeField] private Button button;

    private ItemData _assignedItem;

    private Action<ItemData, HotbarSlot> _onSlotClicked;

    private void Awake()
    {
        if (button == null)
        {
            return;
        }
        button.onClick.AddListener(() => _onSlotClicked?.Invoke(_assignedItem, this));
    }

    public void Setup(ItemData item, int keybindNumber, Action<ItemData, HotbarSlot> clickCallback)
    {
        if (keybindText == null || iconImage == null || seasonImage == null)
        {
            return;
        }
        _assignedItem = item;
        _onSlotClicked = clickCallback;
        keybindText.text = keybindNumber.ToString();

        if (item != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = item.icon;
            
            if (item.requiredSeasonIcon != null)
            {
                seasonImage.enabled = true;
                seasonImage.sprite = item.requiredSeasonIcon;
            }
            else
            {
                seasonImage.enabled = false;
            }
        }
        else
        {
            iconImage.enabled = false;
            seasonImage.enabled = false;
        }
    }
}
