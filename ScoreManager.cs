using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public Text scoreText;     // Drag your ScoreText here in the Inspector

    [Header("Score")]
    public int score = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ResetScore();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}