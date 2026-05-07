using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Rules")]
    [SerializeField] private float gameDuration = 30f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Image timerFill;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameOverSound;

    public bool GameActive { get; private set; }

    private int score;
    private float timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowStartMenu();
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (musicSource != null)
        {
            musicSource.Play();
        }
    }

    private void Update()
    {
        if (!GameActive)
        {
            if (startPanel != null && startPanel.activeSelf && (Input.GetKeyDown(KeyCode.Space) || StartButtonClickedDirectly()))
            {
                StartGame();
            }

            if (gameOverPanel != null && gameOverPanel.activeSelf && (Input.GetKeyDown(KeyCode.R) || RestartButtonClickedDirectly()))
            {
                RestartGame();
            }

            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }

        UpdateTimerUI();
    }

    public void AddScore(int points)
    {
        if (!GameActive)
        {
            return;
        }

        score += points;
        UpdateScoreUI();
    }

    public void StartGame()
    {
        ClearTargets();
        
        TargetSpawner spawner = FindObjectOfType<TargetSpawner>();

        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        score = 0;
        timeRemaining = gameDuration;
        GameActive = true;
        
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = new Color(0.15f, 0.85f, 1f);
        }

        UpdateScoreUI();
        UpdateTimerUI();
    }

    private void ShowStartMenu()
    {
        GameActive = false;
        score = 0;
        timeRemaining = gameDuration;

        if (startPanel != null)
        {
            startPanel.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        UpdateScoreUI();
        UpdateTimerUI();
    }

    private void EndGame()
    {
        GameActive = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = $"Game Over\nFinal Score: {score}";
            gameOverText.gameObject.SetActive(true);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }

        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(gameOverSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}";
        }

        if (timerFill != null)
        {
            float progress = gameDuration > 0f ? timeRemaining / gameDuration : 0f;
            timerFill.fillAmount = progress;
            timerFill.color = Color.Lerp(new Color(1f, 0.18f, 0.12f), new Color(0.15f, 0.85f, 1f), progress);
        }
    }

    public void RestartGame()
    {
        ClearTargets();

        TargetSpawner spawner = FindObjectOfType<TargetSpawner>();

        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.Play();
        }

        StartGame();
    }

    private void ClearTargets()
    {
        ClickTarget[] targets = FindObjectsOfType<ClickTarget>();

        foreach (ClickTarget target in targets)
        {
            Destroy(target.gameObject);
        }
    }

    private bool StartButtonClickedDirectly()
    {
        if (startButton == null || !PointerInput.TryGetPointerDown(out Vector2 pointerPosition))
        {
            return false;
        }

        RectTransform startRect = startButton.GetComponent<RectTransform>();
        return startRect != null && RectTransformUtility.RectangleContainsScreenPoint(startRect, pointerPosition);
    }

    private bool RestartButtonClickedDirectly()
    {
        if (restartButton == null || !PointerInput.TryGetPointerDown(out Vector2 pointerPosition))
        {
            return false;
        }

        RectTransform restartRect = restartButton.GetComponent<RectTransform>();
        return restartRect != null && RectTransformUtility.RectangleContainsScreenPoint(restartRect, pointerPosition);
    }

}
