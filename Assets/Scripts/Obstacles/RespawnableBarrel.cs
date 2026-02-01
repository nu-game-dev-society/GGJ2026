using UnityEngine;
using System.Collections;

public class RespawnableBarrel : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Explosion explosionComponent;
    private BarrelFlashTimer flashTimer;
    private Rigidbody rb;
    private Collider[] colliders;
    private Renderer[] renderers;
    private IHealth healthComponent;
    private bool isRespawning = false;

    private void Awake()
    {
        // Store original transform
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Get components
        explosionComponent = GetComponent<Explosion>();
        flashTimer = GetComponent<BarrelFlashTimer>();
        rb = GetComponent<Rigidbody>();
        healthComponent = GetComponent<IHealth>();
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private IEnumerator RespawnAfterDelay()
    {
        Debug.Log($"{gameObject.name}: Starting respawn sequence");

        // Disable components first to prevent any triggers
        if (flashTimer != null)
        {
            flashTimer.enabled = false;
        }
        if (explosionComponent != null)
        {
            explosionComponent.enabled = false;
        }

        // Disable all visuals and collisions
        SetBarrelActive(false);

        yield return new WaitForSeconds(respawnDelay);

        Debug.Log($"{gameObject.name}: Respawning now");

        // Reset transform
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Reset rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset health to max
        if (healthComponent != null)
        {
            float maxHealth = healthComponent.GetMaxHealth();
            float currentHealth = healthComponent.GetCurrentHealth();
            float healAmount = maxHealth - currentHealth;
            if (healAmount > 0)
            {
                healthComponent.Heal(healAmount);
                Debug.Log($"{gameObject.name}: Health restored to {maxHealth}");
            }
        }

        // Re-enable barrel
        SetBarrelActive(true);

        // Reset and re-enable components
        if (explosionComponent != null)
        {
            explosionComponent.enabled = true;
        }
        if (flashTimer != null)
        {
            flashTimer.ResetTimer();
            flashTimer.enabled = false;
        }

        isRespawning = false;
        Debug.Log($"{gameObject.name}: Respawn complete");
    }

    private void SetBarrelActive(bool active)
    {
        // Toggle renderers
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = active;
            }
        }

        // Toggle colliders
        foreach (var collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = active;
            }
        }
    }

    // Call this instead of letting Explosion destroy the object
    public void OnExplode()
    {
        if (isRespawning)
        {
            Debug.Log($"{gameObject.name}: OnExplode called but already respawning - ignoring");
            return;
        }

        Debug.Log($"{gameObject.name}: OnExplode called - starting respawn");
        isRespawning = true;
        StartCoroutine(RespawnAfterDelay());
    }

    // Public property to check if barrel can explode
    public bool CanExplode()
    {
        return !isRespawning;
    }
}
