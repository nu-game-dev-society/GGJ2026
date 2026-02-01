using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.InputSystem;

public class WhackAttackController : AttackController
{
    [Header("Whack Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileFireDelayInSeconds = 0.5f;
    [SerializeField] private float projectileLifetimeInSeconds = 5f;
    [SerializeField] private float projectileLifetimeAfterValidCollision = 1f;

    public override AttackType AttackType => AttackType.Whack;

    protected override void DetectAndHitEnemies()
    {
        IEnumerator FireProjectileDelayed()
        {
            yield return new WaitForSeconds(projectileFireDelayInSeconds);
            FireProjectile();
        }

        StartCoroutine(FireProjectileDelayed());
    }

    private void FireProjectile()
    {
        Vector3 direction = transform.forward;
        Transform bestPlayer = null;
        float bestDot = 0.8f; // how "central" they must be (cos ~36� cone)

        foreach (PlayerInput input in PlayerInput.all)
        {
            if (input == null) continue;

            Vector3 toPlayer = (input.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toPlayer);

            // Higher dot = more directly in front
            if (dot > bestDot)
            {
                bestDot = dot;
                bestPlayer = input.transform;
            }
        }

        // If we found a good target, aim at them
        if (bestPlayer != null)
        {
            direction = (bestPlayer.position - transform.position).normalized;
        }

        var projectile = Instantiate(projectilePrefab).GetComponent<WhackProjectile>();
        
        projectile.gameObject.transform.position = this.attackPoint.position;
        projectile.Damage = this.attackDamage;
        projectile.Launch(ignoreCollisions.SelectMany(go => go.GetComponentsInChildren<Collider>()).ToArray(), direction, projectileSpeed);

        IEnumerator ResetProjectileDelayed(float secondsToWait)
        {
            yield return new WaitForSeconds(secondsToWait);
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }

        void OnCollided(Collision other)
        {
            ApplyDamage(other.gameObject);
            ApplyKnockback(other.collider, (other.transform.position - transform.position).normalized);

            StartCoroutine(ResetProjectileDelayed(projectileLifetimeAfterValidCollision));
        }

        projectile.Collided += OnCollided;

        StartCoroutine(ResetProjectileDelayed(projectileLifetimeInSeconds));
    }
}