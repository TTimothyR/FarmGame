using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }
    
    [SerializeField] private Canvas startScreenCanvas;
    [SerializeField] private Button playButton;
    
    [SerializeField] private Canvas gameOverScreenCanvas;
    [SerializeField] private Button restartButton;

    [SerializeField] private Canvas winnerScreenCanvas;
    [SerializeField] private Button winnerRestartButton;
    [SerializeField] private TextMeshProUGUI totalScore;
    
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Volume blurVolume;

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
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(BackToStart);
        }
        
        if (winnerRestartButton != null)
        {
            winnerRestartButton.onClick.AddListener(BackToStart);
        }
    }

    private void StartGame()
    {
        GameManager.Instance.StartGame();
        if (startScreenCanvas != null)
        {
            startScreenCanvas.enabled = false;
        }

        if (hudCanvas != null)
        {
            hudCanvas.enabled = true;
        }

        if (blurVolume != null)
        {
            blurVolume.enabled = false;
        }
    }

    public void ShowGameOverScreen()
    {
        if (winnerScreenCanvas != null)
        {
            gameOverScreenCanvas.enabled = true;
        }

        if (hudCanvas != null)
        {
            hudCanvas.enabled = false;
        }

        if (blurVolume != null)
        {
            blurVolume.enabled = true;
        }
    }

    public void ShowWinnerScreen()
    {
        if (winnerScreenCanvas != null)
        {
            winnerScreenCanvas.enabled = true;
        }

        if (totalScore != null)
        {
            totalScore.text = $"Your score: {GameManager.Instance.CurrentScore}";
        }

        if (hudCanvas != null)
        {
            hudCanvas.enabled = false;
        }

        if (blurVolume != null)
        {
            blurVolume.enabled = true;
        }
    }

    private void BackToStart()
    {
        GameManager.Instance.GameOver();
    }
}
