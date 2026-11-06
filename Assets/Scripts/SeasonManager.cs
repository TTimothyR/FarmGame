using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class SeasonManager : MonoBehaviour
{
    public enum Seasons
    {
        Spring,
        Summer,
        Autumn,
        Winter,
        Count
    };

    [System.Serializable]
    private struct Range
    {
        public int minimum;
        public int maximum;
    }

    [System.Serializable]
    private struct Season
    {
        public Seasons type;
        public float dryoutTime;
        public Color color;
        public ItemData seasonalCrop;
        public Range requiredCropsHarvested;
        public int completionBonus;
    }

    [SerializeField] private List<Season> seasonData = new List<Season>();
    [SerializeField] private float seasonDuration = 60 * 1.5f;
    [SerializeField] private GameObject ground;

    
    public static SeasonManager Instance { get; private set; }

    private Seasons currentSeason = Seasons.Spring;
    private float elapsedInThisSeason = 0f;

    private Renderer groundRenderer;
    
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

        if (groundRenderer == null && ground != null)
        {
            groundRenderer = ground.GetComponent<Renderer>();
        }
        SetCurrentSeasonColor();
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted || TutorialManager.Instance.IsTutorialActive)
        {
            return;
        }
        HandleSeason();
    }

    private void HandleSeason()
    {
        elapsedInThisSeason += Time.deltaTime;
        HUD.Instance.UpdateSeasonTimer(seasonDuration - elapsedInThisSeason);

        if (elapsedInThisSeason >= seasonDuration)
        {
            elapsedInThisSeason -= seasonDuration;
            ChangeSeason();
        }
    }

    private void ChangeSeason()
    {
        GameManager.Instance.AwardSeasonBonus();
        if (currentSeason == Seasons.Winter)
        {
            GameManager.Instance.WinGame();
            return;
        }

        currentSeason = (Seasons)(((int)currentSeason + 1) % (int)Seasons.Count);
        if (GameManager.Instance.QuotaProgress < GameManager.Instance.CurrentQuota)
        {
            GameManager.Instance.EndGame("You didn't meet the seasonal quota.");
            return;
        }
        else
        {
            GameManager.Instance.RefreshQuota();
        }
        SetCurrentSeasonColor();
        HUD.Instance.UpdateSeasonIcon(GetCurrentSeasonIcon());
    }

    private void SetCurrentSeasonColor()
    {
        if (groundRenderer != null)
        {
            groundRenderer.material.color = seasonData[(int)currentSeason].color;
        }
    }

    public Seasons GetCurrentSeason()
    {
        return currentSeason;
    }

    private Sprite GetCurrentSeasonIcon()
    {
        return seasonData[(int)currentSeason].seasonalCrop.requiredSeasonIcon;
    }
    
    public float GetDryOutTime()
    {
        return seasonData[(int)currentSeason].dryoutTime;
    }

    public ItemData GetSeasonalCrop()
    {
        return seasonData[(int)currentSeason].seasonalCrop;
    }

    public int GetSeasonalQuota()
    {
        Range seasonalRange = seasonData[(int)currentSeason].requiredCropsHarvested;

        return Random.Range(seasonalRange.minimum, seasonalRange.maximum + 1);
    }

    public int GetSeasonalBonus()
    {
        return seasonData[(int)currentSeason].completionBonus;
    }

    public (float, float) GetElapsedSeasonTime()
    {
        return (elapsedInThisSeason, seasonDuration);
    }
}