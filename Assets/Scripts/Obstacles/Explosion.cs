using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float upwardModifier = 1f; // Adds upward force for more dramatic effect
    [SerializeField] private float characterControllerForceMultiplier = 1f; // CharacterControllers need higher multiplier
    [SerializeField] private float damage = 50f;
    [SerializeField] private LayerMask affectedLayers = ~0; // All layers by default

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;

    [Header("Optional Settings")]
    [SerializeField] private bool destroyAfterExplosion = true;
    [SerializeField] private float destroyDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    [ContextMenu("Explode")]
    public void Explode()
    {
        Vector3 explosionPosition = transform.position;

        // Play particle effect
        if (explosionParticles != null)
        {
            ParticleSystem particles = Instantiate(explosionParticles, explosionPosition, Quaternion.identity);
            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }

        // Play sound effect
        if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Find all colliders in explosion radius
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius, affectedLayers);

        foreach (Collider hit in colliders)
        {
            // Skip self
            if (hit.gameObject == gameObject) continue;

            // Calculate common values for both rigidbodies and character controllers
            Vector3 targetPosition = hit.transform.position;
            float distance = Vector3.Distance(explosionPosition, targetPosition);
            float forceFalloff = 1f - (distance / explosionRadius); // Linear falloff

            // Apply force to rigidbodies
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardModifier, ForceMode.Impulse);
            }

            // Try to apply knockback to PlayerController (CharacterController)
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                // Calculate explosion direction (from explosion to player)
                Vector3 direction = (targetPosition - explosionPosition).normalized;

                // Convert rigidbody force to CharacterController velocity
                // Rigidbody force is divided by mass (assume mass ~1) to get velocity
                // For CharacterController, we need a much smaller value since it's direct movement
                float velocityMagnitude = (explosionForce / 100f) * forceFalloff * characterControllerForceMultiplier;

                // Apply horizontal direction
                Vector3 knockbackVector = direction * velocityMagnitude;

                // Add upward component (separate from direction for more control)
                knockbackVector.y = upwardModifier * velocityMagnitude;

                player.ApplyKnockback(knockbackVector);
            }

            // Apply damage if target has health
            IHealth health = hit.GetComponent<IHealth>();
            if (health != null)
            {
                // Calculate damage falloff based on distance
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = damage * damageFalloff;

                health.TakeDamage(finalDamage);
            }
        }

        // Destroy object after explosion
        if (destroyAfterExplosion)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    public void ExplodeWithCustomForce(float customForce)
    {
        float originalForce = explosionForce;
        explosionForce = customForce;
        Explode();
        explosionForce = originalForce;
    }

    public void ExplodeWithCustomRadius(float customRadius)
    {
        float originalRadius = explosionRadius;
        explosionRadius = customRadius;
        Explode();
        explosionRadius = originalRadius;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Draw inner sphere for max damage zone
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius * 0.5f);
    }
}
