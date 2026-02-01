using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BarrelFlashTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float flashDuration = 3.0f;
    [SerializeField] private float flashSpeed = 5f; // How fast to flash (flashes per second)

    [Header("Flash Settings")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private string colorPropertyName = "_BaseColor"; // URP standard

    [Header("Events")]
    [SerializeField] private UnityEvent onTimerComplete;

    private MaterialPropertyBlock propertyBlock;
    private Color originalColor;
    private float elapsedTime = 0f;
    private bool isFlashing = false;

    private void Awake()
    {
        // Get renderer if not assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.LogError($"BarrelFlashTimer on {gameObject.name} has no Renderer!");
            enabled = false;
            return;
        }

        // Create property block
        propertyBlock = new MaterialPropertyBlock();

        // Store original color
        targetRenderer.GetPropertyBlock(propertyBlock);
        if (propertyBlock.isEmpty)
        {
            // If property block is empty, get color from material
            originalColor = targetRenderer.sharedMaterial.GetColor(colorPropertyName);
        }
        else
        {
            originalColor = propertyBlock.GetColor(colorPropertyName);
        }
    }

    void OnEnable()
    {
        // Start flashing when enabled
        elapsedTime = 0f;
        isFlashing = true;
    }

    private void OnDisable()
    {
        // Reset to original color when disabled
        if (targetRenderer != null && propertyBlock != null)
        {
            propertyBlock.SetColor(colorPropertyName, originalColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    void Update()
    {
        if (!isFlashing) return;

        elapsedTime += UnityEngine.Time.deltaTime;

        // Flash between original and white
        float t = Mathf.PingPong(elapsedTime * flashSpeed, 1f);
        Color currentColor = Color.Lerp(originalColor, flashColor, t);

        // Apply color using property block
        propertyBlock.SetColor(colorPropertyName, currentColor);
        targetRenderer.SetPropertyBlock(propertyBlock);

        // Check if timer is complete
        if (elapsedTime >= flashDuration)
        {
            isFlashing = false;

            // Fire event
            onTimerComplete?.Invoke();
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isFlashing = false;

        // Reset to original color
        if (targetRenderer != null && propertyBlock != null)
        {
            propertyBlock.SetColor(colorPropertyName, originalColor);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
