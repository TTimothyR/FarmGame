using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
/// <summary>
/// Creates the hotbar slots and calls the interact callback when interacted with it.
/// Displays the players current lives, score and high score.
/// Displays the current season icon, with the remaining time in the season, and the seasonal quota.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Quota Warning Animation")] 
    [SerializeField] private float bigScale = 1.5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float quotaNotMetTimeTrigger = 15f;
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

    private readonly List<HotbarSlot> _slotList = new List<HotbarSlot>();
    private TextMeshProUGUI _seasonTimer;

    private readonly Color _quotaNotMetColor = Color.indianRed;
    private readonly Color _quotaMetColor = Color.lightGreen;
    private const float SmallScale = 1f;
    
    
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
            _seasonTimer = currentSeasonIcon.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (quotaLabel != null)
        {
            quotaLabel.color = _quotaNotMetColor;
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
        if (quotaLabel == null || nearDeathVignette == null)
        {
            return;
        }
        (float, float) seasonTimesData = SeasonManager.Instance.GetElapsedSeasonTime();
        if (!GameManager.Instance.IsQuotaMet() && (seasonTimesData.Item2 - seasonTimesData.Item1) <= quotaNotMetTimeTrigger)
        {
            // Scales the quota text bigger and smaller while the quota is not met, as well as playing the low time warning
            AudioManager.Instance.PlayLoop(AudioManager.SoundType.LowTimeWarning);
            float wave = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(SmallScale, bigScale, wave);
            
            quotaLabel.transform.localScale = Vector3.one * scale;
            nearDeathVignette.enabled = true;
        }
        else
        {
            AudioManager.Instance.StopLoop();
            quotaLabel.transform.localScale = Vector3.one * SmallScale;
            nearDeathVignette.enabled = false;
        }
    }

    private void HandleHotbarKeyInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        int slotsToCheck = Mathf.Min(_slotList.Count, HotbarKeys.Length);

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
        if (index < 0 || index >= _slotList.Count)
        {
            return;
        }

        ItemData itemInSlot = (index < slotItems.Count) ? slotItems[index] : null;
        OnSlotPressed(itemInSlot, _slotList[index]);
    }

    private void CreateHotBarSlots()
    {
        if (slotTemplate == null || hotbar == null)
        {
            return;
        }
        for (int i = 0; i < hotbarSlots; ++i)
        {
            HotbarSlot clone = Instantiate(slotTemplate, hotbar.transform);
            ItemData itemInSlot = (i < slotItems.Count) ? slotItems[i] : null;
            
            clone.Setup(itemInSlot, i+1, OnSlotPressed);
            _slotList.Add(clone);
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
            if (isEquipped)
            {
                foreach (HotbarSlot iterateSlot in _slotList)
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

    public void FollowTutorialNotification()
    {
        ShowNotification("Follow the tutorial please!");
    }

    public void ShowNotification(string message)
    {
        ShowNotification(message, NotifBaseColor);
    }
    public void ShowNotification(string message, Color color)
    {
        if (notificationTemplate == null || notificationList == null)
        {
            return;
        }
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
            return;
        }
        
        currentSeasonIcon.sprite = targetIcon;
    }

    public void UpdateSeasonTimer(float remainingSeconds)
    {
        if (_seasonTimer == null)
        {
            return;
        }

        int minutes = (int)Mathf.Floor(remainingSeconds/60f);
        int seconds = (int)(remainingSeconds - (minutes * 60f));

        string minutesStr = "0" + minutes.ToString();
        string secondsStr = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();

        _seasonTimer.text = $"{minutesStr}:{secondsStr}";
    }

    public void UpdateSeasonalQuota()
    {
        if (quotaLabel == null)
        {
            return;
        }

        // Interpolates the seasonal quota text based on how much the quota is completed.
        float progress = Mathf.Clamp01((float)GameManager.Instance.QuotaProgress / GameManager.Instance.CurrentQuota);
        quotaLabel.color = Color.Lerp(_quotaNotMetColor, _quotaMetColor, progress);
        
        quotaLabel.text = $"Quota: {GameManager.Instance.QuotaProgress}/{GameManager.Instance.CurrentQuota}";
    }
}
