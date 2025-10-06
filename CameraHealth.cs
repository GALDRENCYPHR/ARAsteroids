// CameraHealth.cs — unified health + hearts UI + game over
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 2;              // total hearts
    [SerializeField] private int health;   // current hearts (runtime)

    [Header("Hearts UI")]
    public GameObject heartPrefab;         // UI Image prefab with your heart sprite
    public Transform heartsParent;         // a RectTransform under your Canvas (e.g., "HeartsParent")
    public bool rebuildHeartsEachStart = true; // rebuild at Start (leave on unless you manage hearts manually)

    [Header("Game Over UI")]
    public Text gameOverText;              // optional legacy Text for "Game Over!"
    public GameObject gameOverPanel;       // panel with your Restart button

    [Header("Options")]
    public bool freezeTimeOnGameOver = true;

    // internal cache
    private readonly List<Image> _heartImages = new List<Image>();

    void Start()
    {
        // Ensure gameplay is running
        // Time.timeScale = 1f;

        // UI defaults
        if (gameOverText != null) gameOverText.enabled = false;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Initialize health and hearts UI
        health = Mathf.Max(0, maxHealth);
        if (rebuildHeartsEachStart) BuildHeartsUI();
        UpdateHeartsUI();
    }

    // Build heart icons once
    void BuildHeartsUI()
    {
        if (heartsParent == null || heartPrefab == null)
        {
            Debug.LogWarning("[CameraHealth] Hearts UI not set: assign heartPrefab and heartsParent.");
            return;
        }

        // Clear any existing children if you want a clean rebuild
        for (int i = heartsParent.childCount - 1; i >= 0; i--)
            Destroy(heartsParent.GetChild(i).gameObject);
        _heartImages.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            var heart = Instantiate(heartPrefab, heartsParent);
            var img = heart.GetComponent<Image>();
            if (img == null)
            {
                img = heart.AddComponent<Image>(); // fallback if prefab wasn't an Image
            }
            _heartImages.Add(img);
        }
    }

    // Toggle which hearts are visible based on current health
    void UpdateHeartsUI()
    {
        if (_heartImages.Count == 0) return;

        for (int i = 0; i < _heartImages.Count; i++)
        {
            bool shouldShow = i < health;
            if (_heartImages[i] != null)
                _heartImages[i].enabled = shouldShow;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= Mathf.Abs(damage);
        health = Mathf.Clamp(health, 0, maxHealth);

        UpdateHeartsUI();

        Debug.Log($"[CameraHealth] TakeDamage({damage}). Health now = {health}");
        if (health <= 0) EndGame();
    }

    void EndGame()
    {
        Debug.Log("[CameraHealth] Game Over triggered.");

        // Show UI
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over!";
            gameOverText.enabled = true;
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Stop all spawners and their coroutines
        var spawners = FindObjectsByType<SpawnScript>(FindObjectsSortMode.None);
        foreach (var s in spawners)
        {
            s.StopAllCoroutines();
            s.enabled = false;
        }

        // Disable shooting
        var shooters = FindObjectsByType<ShootScript>(FindObjectsSortMode.None);
        foreach (var sh in shooters) sh.enabled = false;

        // Freeze existing rocks
        var rocks = FindObjectsByType<RockScript>(FindObjectsSortMode.None);
        foreach (var r in rocks)
        {
            r.enabled = false; // stop movement
            var rb = r.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // NOTE: use 'velocity' (not 'linearVelocity')
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }

        if (freezeTimeOnGameOver) Time.timeScale = 0f;
    }

    // Hook this to your Restart button via Inspector → Button (On Click)
    public void Restart()
    {
        Debug.Log("[CameraHealth] Restart pressed.");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}