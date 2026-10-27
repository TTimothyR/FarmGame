using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject notificationList;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private TextMeshProUGUI notificationTemplate;
    [SerializeField] private TextMeshProUGUI lifeCounter;
    [SerializeField] private TextMeshProUGUI scoreCounter;
    [SerializeField] private TextMeshProUGUI seasonTimer;
    [SerializeField] private HotbarSlot slotTemplate;
    [SerializeField] private int hotbarSlots = 5;
    [SerializeField] private float notificationDuration = 5f;
    [SerializeField] private List<ItemData> slotItems;

    private List<HotbarSlot> slotList = new List<HotbarSlot>();
    
    // The HUD is a singleton
    public static HUD Instance { get; private set; }

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

    // Load the UI hotbar slots at the start of the game, based on how many slots are needed.
    private void Start()
    {
        CreateHotBarSlots();
    }

    private void CreateHotBarSlots()
    {
        for (int i = 0; i < hotbarSlots; ++i)
        {
            HotbarSlot clone = Instantiate(slotTemplate, hotbar.transform);
            ItemData itemInSlot = (i < slotItems.Count) ? slotItems[i] : null;
            
            clone.Setup(itemInSlot, i+1, OnSlotPressed);
            slotList.Add(clone);
        }
    }

    private void SetSlotColor(HotbarSlot slot, bool isEquipped)
    {
        Image background = slot.GetComponent<Image>();
        if (background != null)
        {
            background.color = isEquipped ? Color.mediumSeaGreen : Color.white;
        }
    }

    private void OnSlotPressed(ItemData selectedItem, HotbarSlot slot)
    {
        if (selectedItem != null)
        {
            bool isEquipped = Inventory.Instance.ToggleItem(selectedItem);
            if (isEquipped == true)
            {
                foreach (HotbarSlot iterateSlot in slotList)
                {
                    if (iterateSlot != slot)
                    {
                        SetSlotColor(iterateSlot, false);
                    }
                }
            }
            SetSlotColor(slot, isEquipped);
        }
    }

    private static readonly Color NotifBaseColor = new Color(0.85f, 0.26f, 0.25f);

    // Shows a notification on screen for 5 seconds with either the base color or desired color.
    public void ShowNotification(string message)
    {
        ShowNotification(message, NotifBaseColor);
    }
    public void ShowNotification(string message, Color color)
    {
        TextMeshProUGUI clone = Instantiate(notificationTemplate, notificationList.transform);
        clone.text = message;
        clone.color = color;
        
        Destroy(clone.gameObject, notificationDuration);
    }

    public void UpdateLives()
    {
        if (lifeCounter == null)
        {
            return;
        }

        lifeCounter.text = $"Lives: {GameManager.Instance.CurrentLives}";
    }

    public void UpdateScore()
    {
        if (scoreCounter == null)
        {
            return;
        }

        scoreCounter.text = $"Score: {GameManager.Instance.CurrentScore}";
    }

    public void UpdateSeasonTimer(float remainingSeconds)
    {
        if (seasonTimer == null)
        {
            return;
        }

        int minutes = (int)Mathf.Floor(remainingSeconds/60f);
        int seconds = (int)(remainingSeconds - (minutes * 60f));

        if (minutes == 0)
        {
            seasonTimer.text = $"Season changes in: {seconds}s";
        }
        else
        {
            seasonTimer.text = $"Season changes in: {minutes}m {seconds}s";
        }
    }
}
