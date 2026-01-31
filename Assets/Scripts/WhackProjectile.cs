using UnityEngine;

public class WhackProjectile : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpwardForce = 2f;

    public float Damage { get; set; }

    public void Launch(Vector3 direction, float speed)
    {
        rigidBody.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == gameObject) return;

        ApplyDamage(other.gameObject);
        ApplyKnockback(other.gameObject, (other.transform.position - transform.position).normalized);
    }

    private void ApplyDamage(GameObject victim)
    {
        // Try to find health component (you can adjust this based on your health system)
        var healthComponent = victim.GetComponent<IHealth>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(Damage);
        }
    }

    private void ApplyKnockback(GameObject target, Vector3 direction)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force in the direction with upward component for Gang Beasts style physics
            Vector3 knockbackDirection = new Vector3(direction.x, 0, direction.z).normalized;
            Vector3 knockbackVector = knockbackDirection * knockbackForce + Vector3.up * knockbackUpwardForce;

            rb.AddForce(knockbackVector, ForceMode.Impulse);
        }
    }
}