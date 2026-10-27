using UnityEngine;

public class SeasonManager : MonoBehaviour
{
    public enum Seasons
    {
        Spring,
        Summer,
        Autumn,
        Winter
    };
    
    [System.Serializable]
    public struct SeasonDryoutTimes
    {
        public float springTime;
        public float summerTime;
        public float autumnTime;
        public float winterTime;
    }
    [SerializeField] private SeasonDryoutTimes dryOutTimes = new SeasonDryoutTimes
    {
        springTime = 10f,
        summerTime = 5f,
        autumnTime = 12f,
        winterTime = 20f
    };
    
    public static SeasonManager Instance { get; private set; }

    private Seasons currentSeason = Seasons.Spring;
    
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

    public Seasons GetCurrentSeason()
    {
        return currentSeason;
    }
    
    public float GetDryOutTime() => currentSeason switch
    {
        Seasons.Spring => dryOutTimes.springTime,
        Seasons.Summer => dryOutTimes.summerTime,
        Seasons.Autumn => dryOutTimes.autumnTime,
        Seasons.Winter => dryOutTimes.winterTime,
        _ => 0f
    };
}
