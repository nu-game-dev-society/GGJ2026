using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    [Header("Life Indicators")]
    [SerializeField] private GameObject[] lifeIndicators = new GameObject[3];

    private int currentLives = 3;

    public void Initialize(Color playerColor)
    {
        Debug.Log($"PlayerMarker.Initialize called with color: {playerColor}");

        // Set all life indicators to the player's color
        int indicatorCount = 0;
        foreach (var indicator in lifeIndicators)
        {
            if (indicator != null)
            {
                Renderer renderer = indicator.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Create a new material instance to avoid sharing materials
                    renderer.material = new Material(renderer.material);
                    renderer.material.color = playerColor;
                    indicatorCount++;
                    Debug.Log($"Set color for life indicator {indicatorCount}: {indicator.name}");
                }
                else
                {
                    Debug.LogWarning($"Life indicator {indicator.name} has no Renderer component!");
                }
            }
            else
            {
                Debug.LogWarning("Life indicator is null!");
            }
        }

        // Enable all life indicators
        currentLives = lifeIndicators.Length;
        foreach (var indicator in lifeIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(true);
            }
        }

        Debug.Log($"Initialized {indicatorCount} life indicators with color {playerColor}");
    }

    public void LoseLife()
    {
        if (currentLives > 0)
        {
            currentLives--;

            // Disable the life indicator at the current lives index
            if (currentLives >= 0 && currentLives < lifeIndicators.Length)
            {
                if (lifeIndicators[currentLives] != null)
                {
                    lifeIndicators[currentLives].SetActive(false);
                    Debug.Log($"Life indicator {currentLives} disabled. Lives remaining: {currentLives}");
                }
            }
        }
    }

    public void ResetLives()
    {
        currentLives = lifeIndicators.Length;

        // Re-enable all life indicators
        foreach (var indicator in lifeIndicators)
        {
            if (indicator != null)
            {
                indicator.SetActive(true);
            }
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}
