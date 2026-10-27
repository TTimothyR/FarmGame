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

    public void LoseLife()
    {
        --CurrentLives;
        HUD.Instance.UpdateLives();
        if (CurrentLives <= 0)
        {
            ScreenManager.Instance.ShowGameOverScreen();
            GameStarted = false;
            // GameOver();
        }
    }

    public void GainScore()
    {
        ++CurrentScore;
        HUD.Instance.UpdateScore();
    }

    public void StartGame()
    {
        GameStarted = true;
    }

    public void GameOver()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
