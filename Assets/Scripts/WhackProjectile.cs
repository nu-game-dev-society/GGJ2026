using UnityEngine;
using System.Collections.Generic;

public class WhackProjectile : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpwardForce = 2f;
    [SerializeField] protected float characterControllerKnockbackMultiplier = 5f;

    public float Damage { get; set; }

    public void Launch(IReadOnlyCollection<Collider> collidersToIgnore, Vector3 direction, float speed)
    {
        foreach (var colliderToIgnore in collidersToIgnore)
        {
            foreach (var myCollider in this.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(myCollider, colliderToIgnore);
            }
        }
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

        Debug.Log($"ApplyKnockback({target.name})");
        Rigidbody rb = target.GetComponent<Rigidbody>();
        Vector3 knockbackDirection = new Vector3(direction.x, 0, direction.z).normalized;
        if (rb != null)
        {
            Vector3 knockbackVector = knockbackDirection * knockbackForce + Vector3.up * knockbackUpwardForce;

            rb.AddForce(knockbackVector, ForceMode.Impulse);
            return;
        }

        // Try to apply knockback to PlayerController (CharacterController)
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("PlayerFound: " + player.name);
            // CharacterControllers need a higher multiplier since they use direct movement
            // Also need to apply over time, not as impulse
            Vector3 knockbackVector = knockbackDirection * knockbackForce * characterControllerKnockbackMultiplier
                                    + Vector3.up * knockbackUpwardForce * characterControllerKnockbackMultiplier;

            player.ApplyKnockback(knockbackVector);
        }
    }
}