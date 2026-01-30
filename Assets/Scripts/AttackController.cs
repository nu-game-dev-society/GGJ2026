using UnityEngine;
using System.Collections.Generic;

public class AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 90f; // Cone angle in degrees
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpwardForce = 2f; // Upward component for dynamic feel
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform attackPoint; // Optional: pivot point for attack origin
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private string attackAnimationTrigger = "Attack";

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private float lastAttackTime = -999f;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (attackPoint == null)
        {
            attackPoint = transform;
        }
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    [ContextMenu("Attack")]
    public void PerformAttack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger(attackAnimationTrigger);
        }

        // Detect and damage enemies in cone
        DetectAndHitEnemies();
    }

    private void DetectAndHitEnemies()
    {
        Vector3 attackOrigin = attackPoint.position;
        Vector3 attackDirection = transform.forward; // Use character's forward direction

        // Get all colliders in range
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange, enemyLayer);

        List<GameObject> hitEnemies = new List<GameObject>();

        foreach (Collider collider in hitColliders)
        {
            // Skip self
            if (collider.gameObject == gameObject) continue;

            // Check if target is within attack cone
            Vector3 directionToTarget = (collider.transform.position - attackPoint.position).normalized;

            // Calculate angle on the horizontal plane (XZ) for top-down style games
            Vector3 attackDirFlat = new Vector3(attackDirection.x, 0, attackDirection.z).normalized;
            Vector3 targetDirFlat = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;

            float angleToTarget = Vector3.Angle(attackDirFlat, targetDirFlat);

            if (angleToTarget <= attackAngle / 2f)
            {
                hitEnemies.Add(collider.gameObject);

                // Apply damage
                ApplyDamage(collider.gameObject);

                // Apply knockback
                ApplyKnockback(collider, directionToTarget);
            }
        }
    }

    private void ApplyDamage(GameObject target)
    {
        // Try to find health component (you can adjust this based on your health system)
        var healthComponent = target.GetComponent<IHealth>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(attackDamage);
        }

    }

    private void ApplyKnockback(Collider target, Vector3 direction)
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

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Vector3 origin = attackPoint != null ? attackPoint.position : transform.position;
        Vector3 direction = transform.forward;

        // Draw attack range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, attackRange);

        // Draw attack cone on horizontal plane
        Gizmos.color = Color.red;

        // Flatten direction to XZ plane for top-down visualization
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z).normalized;

        Vector3 rightBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * flatDirection * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * flatDirection * attackRange;

        Gizmos.DrawLine(origin, origin + rightBoundary);
        Gizmos.DrawLine(origin, origin + leftBoundary);
        Gizmos.DrawLine(origin, origin + flatDirection * attackRange);

        // Draw arc to visualize cone better
        int segments = 10;
        Vector3 previousPoint = origin + rightBoundary;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -attackAngle / 2f + (attackAngle * i / segments);
            Vector3 point = origin + Quaternion.Euler(0, angle, 0) * flatDirection * attackRange;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}

// Interface for health system (create this if you don't have a health system yet)
public interface IHealth
{
    void TakeDamage(float damage);
}
