using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackAngle = 90f; // Cone angle in degrees
    [SerializeField] protected float knockbackForce = 10f;
    [SerializeField] protected float knockbackUpwardForce = 2f; // Upward component for dynamic feel
    [SerializeField] protected float characterControllerKnockbackMultiplier = 5f; // CharacterControllers need higher multiplier for similar effect
    [SerializeField] protected float attackCooldown = 0.5f;
    [SerializeField] protected float attackDelay = 0.0f; // Delay before raycast/hit detection
    [SerializeField] protected List<GameObject> ignoreCollisions;
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] protected Transform attackPoint; // Optional: pivot point for attack origin
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    public virtual AttackType AttackType => AttackType.None;

    private float lastAttackTime = -999f;
    private float damageModifier = 1f;

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

    public void SetDamageModifier(float modifier)
    {
        damageModifier = Mathf.Max(0, modifier);
    }
    [ContextMenu("Attack")]
    public void PerformAttack()
    {
        if (!CanAttack()) return;

        lastAttackTime = Time.time;

        // Trigger animation immediately
        if (animator != null && AttackType != AttackType.None)
        {
            animator.SetTrigger(AttackType.ToString());
        }

        // Delay the actual attack detection
        StartCoroutine(DelayedAttack());
    }

    private IEnumerator DelayedAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        // Detect and damage enemies in cone after delay
        DetectAndHitEnemies();
    }

    protected virtual void DetectAndHitEnemies()
    {
        Debug.LogError($"DetectAndHitEnemies not implemented in {nameof(AttackController)} subclass!");
    }

    protected void ApplyDamage(GameObject target)
    {
        // Try to find health component (you can adjust this based on your health system)
        var healthComponent = target.GetComponent<IHealth>();
        if (healthComponent != null)
        {
            float finalDamage = attackDamage * damageModifier;
            healthComponent.TakeDamage(finalDamage);
        }

    }

    protected void ApplyKnockback(Collider target, Vector3 direction)
    {
        // Normalize horizontal direction
        Vector3 knockbackDirection = new Vector3(direction.x, 0, direction.z).normalized;

        // Try to apply knockback to Rigidbody
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force with upward component for Gang Beasts style physics
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
