using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class HUD : MonoBehaviour
{
    [Header("Quota Warning Animation")] 
    [SerializeField] private float bigScale = 1.5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private Volume nearDeathVignette;
    
    [SerializeField] private GameObject notificationList;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private TextMeshProUGUI notificationTemplate;
    [SerializeField] private TextMeshProUGUI lifeCounter;
    [SerializeField] private TextMeshProUGUI scoreCounter;
    [SerializeField] private TextMeshProUGUI highScoreCounter;
    [SerializeField] private TextMeshProUGUI quotaLabel;
    [SerializeField] private Image currentSeasonIcon;
    [SerializeField] private HotbarSlot slotTemplate;
    [SerializeField] private int hotbarSlots = 5;
    [SerializeField] private float notificationDuration = 5f;
    [SerializeField] private List<ItemData> slotItems;

    private List<HotbarSlot> slotList = new List<HotbarSlot>();
    private TextMeshProUGUI seasonTimer;

    private Color quotaNotMetColor = Color.indianRed;
    private Color quotaMetColor = Color.lightGreen;
    private float smallScale = 1f;
    
    
    public static HUD Instance { get; private set; }

    private static readonly Key[] HotbarKeys = new Key[]
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
    };

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

        if (currentSeasonIcon != null)
        {
            seasonTimer = currentSeasonIcon.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (quotaLabel != null)
        {
            quotaLabel.color = quotaNotMetColor;
        }
    }

    private void Start()
    {
        CreateHotBarSlots();
    }

    private void Update()
    {
        HandleHotbarKeyInput();
        QuotaWarningAnimation();
    }

    private void QuotaWarningAnimation()
    {
        (float, float) seasonTimesData = SeasonManager.Instance.GetElapsedSeasonTime();
        if (!GameManager.Instance.IsQuotaMet() && (seasonTimesData.Item2 - seasonTimesData.Item1) <= 15f)
        {
            float wave = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(smallScale, bigScale, wave);
            quotaLabel.transform.localScale = Vector3.one * scale;
            nearDeathVignette.enabled = true;
        }
        else
        {
            quotaLabel.transform.localScale = Vector3.one * smallScale;
            nearDeathVignette.enabled = false;
        }
    }

    private void HandleHotbarKeyInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        int slotsToCheck = Mathf.Min(slotList.Count, HotbarKeys.Length);

        for (int index = 0; index < slotsToCheck; index++)
        {
            if (Keyboard.current[HotbarKeys[index]].wasPressedThisFrame)
            {
                SelectSlotByIndex(index);
                break;
            }
        }
    }

    private void SelectSlotByIndex(int index)
    {
        if (index < 0 || index >= slotList.Count)
        {
            return;
        }

        ItemData itemInSlot = (index < slotItems.Count) ? slotItems[index] : null;
        OnSlotPressed(itemInSlot, slotList[index]);
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

    public void UpdateHighScore()
    {
        if (highScoreCounter == null)
        {
            return;
        }

        highScoreCounter.text = $"HighScore: {GameManager.Instance.CurrentHighScore}";
    }

    public void UpdateSeasonIcon(Sprite targetIcon)
    {
        if (currentSeasonIcon != null)
        {
            currentSeasonIcon.sprite = targetIcon;
        }
    }

    public void UpdateSeasonTimer(float remainingSeconds)
    {
        if (seasonTimer == null)
        {
            return;
        }

        int minutes = (int)Mathf.Floor(remainingSeconds/60f);
        int seconds = (int)(remainingSeconds - (minutes * 60f));

        string minutesStr = "0" + minutes.ToString();
        string secondsStr = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();

        seasonTimer.text = $"{minutesStr}:{secondsStr}";
    }

    public void UpdateSeasonalQuota()
    {
        if (quotaLabel == null)
        {
            return;
        }

        float progress = Mathf.Clamp01((float)GameManager.Instance.QuotaProgress / GameManager.Instance.CurrentQuota);
        quotaLabel.color = Color.Lerp(quotaNotMetColor, quotaMetColor, progress);
        
        quotaLabel.text = $"Quota: {GameManager.Instance.QuotaProgress}/{GameManager.Instance.CurrentQuota}";
    }
}
