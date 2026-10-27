using System;
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
        if (gameOverScreenCanvas != null)
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

    private void BackToStart()
    {
        GameManager.Instance.GameOver();
    }
}
