using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// Manages which screen is shown and what happens when the buttons on the screens are pressed.
/// </summary>
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }
    
    [Header("Start Screen")]
    [SerializeField] private Canvas startScreenCanvas;
    [SerializeField] private Button playButton;
    [SerializeField] private VerticalLayoutGroup leaderboard;
    [SerializeField] private TextMeshProUGUI leaderboardEntryTemplate;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Toggle tutorialToggle;
    [SerializeField] private float playErrorDisplayTime;
    
    [Header("Game Over Screen")]
    [SerializeField] private Canvas gameOverScreenCanvas;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI reasonLabel;

    [Header("Winner Screen")]
    [SerializeField] private Canvas winnerScreenCanvas;
    [SerializeField] private Button winnerRestartButton;
    [SerializeField] private TextMeshProUGUI totalScore;
    
    [Header("Misc")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Volume blurVolume;

    private TextMeshProUGUI _playText;

    private float _elapsedDisplayTime;
    private bool _displayingError;
    
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
            _playText = playButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(BackToStart);
        }
        
        if (winnerRestartButton != null)
        {
            winnerRestartButton.onClick.AddListener(BackToStart);
        }
        
        DisplayLeaderboard();
    }

    private void Update()
    {
        if (_displayingError)
        {
            _elapsedDisplayTime += Time.deltaTime;

            if (_elapsedDisplayTime >= playErrorDisplayTime)
            {
                _displayingError = false;
                _elapsedDisplayTime = 0f;

                if (_playText != null)
                {
                    _playText.text = "Play";
                }
            }
        }
    }

    private void DisplayLeaderboard()
    {
        if (leaderboardEntryTemplate == null || leaderboard == null)
        {
            return;
        }
        List<Entry> topScores = LeaderboardManager.GetTopScores();

        for (int index = 0; index < topScores.Count; index++)
        {
            TextMeshProUGUI clone = Instantiate(leaderboardEntryTemplate, leaderboard.transform);
            clone.text = $"{index + 1}. {topScores[index].playerName} - {topScores[index].score}";
        }
    }

    private void StartGame()
    {
        if (playerNameInput == null)
        {
            return;
        }
        if (playerNameInput.text == "")
        {
            if (_playText != null)
            {
                _playText.text = "Enter a name!";
                _displayingError = true;
                _elapsedDisplayTime = 0f;
            }
            return;
        }

        if (tutorialToggle != null)
        {
            if (tutorialToggle.isOn)
            {
                TutorialManager.Instance.StartTutorial();
            }
        }

        // Cuts out white spaces at the end of the input string.
        LeaderboardManager.CurrentPlayerName = playerNameInput.text.Trim();
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

    public void ShowGameOverScreen(string message)
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

        if (reasonLabel != null)
        {
            reasonLabel.text = message;
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
