using UnityEngine;
using System.Collections.Generic;

public class SlashAttackController : AttackController
{
    public override AttackType AttackType => AttackType.Slash;

    protected override void DetectAndHitEnemies()
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
}