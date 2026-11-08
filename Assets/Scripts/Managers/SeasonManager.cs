using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
/// <summary>
/// Manages the season data based on a season type to avoid typos and other variables.
/// It tells the game when the season changes and when the seasonal quota should be checked or refreshed.
/// </summary>
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

    private Seasons _currentSeason = Seasons.Spring;
    private float _elapsedInThisSeason;

    private Renderer _groundRenderer;
    
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

        if (_groundRenderer == null && ground != null)
        {
            _groundRenderer = ground.GetComponent<Renderer>();
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
        _elapsedInThisSeason += Time.deltaTime;
        HUD.Instance.UpdateSeasonTimer(seasonDuration - _elapsedInThisSeason);

        if (_elapsedInThisSeason >= seasonDuration)
        {
            _elapsedInThisSeason -= seasonDuration;
            ChangeSeason();
        }
    }

    private void ChangeSeason()
    {
        GameManager.Instance.AwardSeasonBonus();
        if (_currentSeason == Seasons.Winter)
        {
            GameManager.Instance.WinGame();
            return;
        }

        _currentSeason = (Seasons)(((int)_currentSeason + 1) % (int)Seasons.Count);
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
        if (_groundRenderer != null)
        {
            _groundRenderer.material.color = seasonData[(int)_currentSeason].color;
        }
    }

    public Seasons GetCurrentSeason()
    {
        return _currentSeason;
    }

    private Sprite GetCurrentSeasonIcon()
    {
        return seasonData[(int)_currentSeason].seasonalCrop.requiredSeasonIcon;
    }
    
    public float GetDryOutTime()
    {
        return seasonData[(int)_currentSeason].dryoutTime;
    }

    public ItemData GetSeasonalCrop()
    {
        return seasonData[(int)_currentSeason].seasonalCrop;
    }

    public int GetSeasonalQuota()
    {
        Range seasonalRange = seasonData[(int)_currentSeason].requiredCropsHarvested;

        return Random.Range(seasonalRange.minimum, seasonalRange.maximum + 1);
    }

    public int GetSeasonalBonus()
    {
        return seasonData[(int)_currentSeason].completionBonus;
    }

    public (float, float) GetElapsedSeasonTime()
    {
        return (_elapsedInThisSeason, seasonDuration);
    }
}