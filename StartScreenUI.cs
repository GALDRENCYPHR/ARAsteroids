using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject startPanel;     // full-screen panel
    public Text messageText;          // Legacy Text
    [TextArea] public string message = "Can you get to 300 points?\nTap Start when you're ready.";

    [Header("Other UI to toggle")]
    public GameObject gameplayGroup;  // parent with ScoreText + HeartsParent, etc.
    public GameObject gameOverPanel;  // your existing game over panel (optional)

    void Awake()
    {
        // Show start, hide gameplay + game over
        if (startPanel) startPanel.SetActive(true);
        if (gameplayGroup) gameplayGroup.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (messageText) messageText.text = message;

        // Pause everything
        Time.timeScale = 0f;
    }

    // Hook this to StartButton.onClick
    public void StartGame()
    {
        if (startPanel) startPanel.SetActive(false);
        if (gameplayGroup) gameplayGroup.SetActive(true);

        // Optional: reset score
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();

        Time.timeScale = 1f;
    }
}