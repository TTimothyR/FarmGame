using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int maxLives = 3;
    [SerializeField] private int victoryBonus = 50;
    [SerializeField] private int lifeBonus = 10;
    
    public int CurrentLives { get; private set; }
    public int CurrentScore { get; private set; }
    public int CurrentHighScore { get; private set; }
    public int CurrentQuota { get; private set; }
    public int QuotaProgress { get; private set; }
    public bool GameStarted { get; private set; }
    
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

        CurrentLives = maxLives;
        CurrentScore = 0;
    }

    private void Start()
    {
        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateLives();
            HUD.Instance.UpdateScore();
        }
    }

    public void EndGame(string message)
    {
        ScreenManager.Instance.ShowGameOverScreen(message);
        GameStarted = false;
    }

    public void RefreshQuota()
    {
        QuotaProgress = 0;
        CurrentQuota = SeasonManager.Instance.GetSeasonalQuota();
        HUD.Instance.UpdateSeasonalQuota();
    }

    public void ResetTutorialValues()
    {
        QuotaProgress = 0;
        CurrentScore = 0;
        
        HUD.Instance.UpdateScore();
        HUD.Instance.UpdateSeasonalQuota();
    }

    public void LoseLife()
    {
        --CurrentLives;
        HUD.Instance.UpdateLives();
        if (CurrentLives <= 0)
        {
            EndGame("Too many crops were destroyed.");
        }
    }

    public void AwardSeasonBonus()
    {
        int bonus = SeasonManager.Instance.GetSeasonalBonus();
        CurrentScore += bonus;
        HUD.Instance.UpdateScore();
        HUD.Instance.ShowNotification($"+{bonus} Season Completed Bonus!");

        if (CurrentScore > CurrentHighScore)
        {
            CurrentHighScore = CurrentScore;
            HUD.Instance.UpdateHighScore();
        }
    }

    public bool IsQuotaMet()
    {
        return QuotaProgress >= CurrentQuota;
    }

    public void GainScore()
    {
        ++CurrentScore;
        ++QuotaProgress;
        HUD.Instance.UpdateScore();
        HUD.Instance.UpdateSeasonalQuota();
        
        if (CurrentScore > CurrentHighScore && !TutorialManager.Instance.IsTutorialActive)
        {
            CurrentHighScore = CurrentScore;
            HUD.Instance.UpdateHighScore();
        }
    }

    public void BadInsectKillBonus()
    {
        ++CurrentScore;
        HUD.Instance.UpdateScore();
        
        if (CurrentScore > CurrentHighScore && !TutorialManager.Instance.IsTutorialActive)
        {
            CurrentHighScore = CurrentScore;
            HUD.Instance.UpdateHighScore();
        }
    }

    public void GoodInsectKillPunishment()
    {
        if (CurrentScore == CurrentHighScore)
        {
            CurrentHighScore = CurrentScore - 1;
            HUD.Instance.UpdateHighScore();
        }
        --CurrentScore;
        HUD.Instance.UpdateScore();
    }

    public void StartGame()
    {
        GameStarted = true;

        CurrentHighScore = LeaderboardManager.GetPlayerHighScore(LeaderboardManager.CurrentPlayerName);
        RefreshQuota();

        if (HUD.Instance != null)
        {
            HUD.Instance.UpdateHighScore();
        }
    }

    public void WinGame()
    {
        int totalBonus = victoryBonus + (CurrentLives * lifeBonus);

        CurrentScore += totalBonus;
        HUD.Instance.UpdateScore();
        HUD.Instance.ShowNotification($"You Won! +{victoryBonus} Win Bonus, +{lifeBonus} Life Bonus!");
        
        if (CurrentScore > CurrentHighScore)
        {
            CurrentHighScore = CurrentScore;
            HUD.Instance.UpdateHighScore();
        }
        
        ScreenManager.Instance.ShowWinnerScreen();
        GameStarted = false;
    }

    public void GameOver()
    {
        LeaderboardManager.SaveScore(LeaderboardManager.CurrentPlayerName, CurrentScore);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
