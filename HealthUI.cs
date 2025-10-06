using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public int maxHealth = 3;       // How many hearts total
    public int currentHealth;       // Current health value
    public GameObject heartPrefab;  // The heart UI prefab
    public Transform heartsParent;  // Where the hearts will be placed in the Canvas

    private List<GameObject> hearts = new List<GameObject>();

    void Start()
    {
        currentHealth = maxHealth;
        DrawHearts();
    }

    void DrawHearts()
    {
        // Clear old hearts
        foreach (GameObject heart in hearts)
        {
            Destroy(heart);
        }
        hearts.Clear();

        // Draw hearts based on current health
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartsParent);
            hearts.Add(newHeart);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        DrawHearts();

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over!");
            // Call your game over logic here
        }
    }
}