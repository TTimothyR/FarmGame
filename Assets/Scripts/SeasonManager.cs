using UnityEngine;
using System.Collections.Generic;
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
    private struct Season
    {
        public Seasons type;
        // public float duration;
        public float rainFrequency; // After how many seconds does it rain?
        public float rainDuration; // How long does it rain for?
        public float dryoutTime;
        public Color color;
        public ItemData seasonalCrop;
    }

    [SerializeField] private List<Season> seasonData = new List<Season>();
    [SerializeField] private GameObject ground;

    
    public static SeasonManager Instance { get; private set; }

    private Seasons currentSeason = Seasons.Spring;
    private const float seasonDuration = 60 * 1.5f; // 60 seconds x 4 minutes
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
        if (!GameManager.Instance.GameStarted)
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
        currentSeason = (Seasons)(((int)currentSeason + 1) % (int)Seasons.Count);
        SetCurrentSeasonColor();
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
    
    public float GetDryOutTime()
    {
        return seasonData[(int)currentSeason].dryoutTime;
    }

    public ItemData GetSeasonalCrop()
    {
        return seasonData[(int)currentSeason].seasonalCrop;
    }
}