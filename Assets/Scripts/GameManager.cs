using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int maxLives = 3;
    
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

    public void LoseLife()
    {
        --CurrentLives;
        HUD.Instance.UpdateLives();
        if (CurrentLives <= 0)
        {
            EndGame("Too many crops were destroyed.");
        }
    }

    public void GainScore()
    {
        ++CurrentScore;
        ++QuotaProgress;
        HUD.Instance.UpdateScore();
        HUD.Instance.UpdateSeasonalQuota();
        
        if (CurrentScore > CurrentHighScore)
        {
            CurrentHighScore = CurrentScore;
            HUD.Instance.UpdateHighScore();
        }
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
