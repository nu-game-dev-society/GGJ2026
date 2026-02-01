using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class BlastAttackController : AttackController
{
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Blast Projectile")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileFireDelayInSeconds = 0.5f;
    [SerializeField] private float projectileLifetimeInSeconds = 2f;
    [SerializeField] private float projectileLifetimeAfterValidCollision = 0.1f;

    public override AttackType AttackType => AttackType.Blast;

    private void Update()
    {
        if (projectile != null && projectile.gameObject.activeSelf)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[]{this.gameObject.transform.position, projectile.gameObject.transform.position});
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void OnDisable()
    {
        lineRenderer.enabled = false;
    }

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
        if (projectile.gameObject.activeSelf)
        {
            return;
        }

        Vector3 direction = transform.forward;
        Transform bestPlayer = null;
        float bestDot = 0.7f; // how "central" they must be (cos ~36° cone)

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

        projectile.gameObject.SetActive(true);
        projectile.transform.SetParent(null);

        var projectileComp = projectile.AddComponent<WhackProjectile>();
        projectileComp.gameObject.transform.position = this.attackPoint.position;
        projectileComp.gameObject.transform.rotation = Quaternion.LookRotation(direction * -1f);// idk why we need to flip this but we do
        projectileComp.Damage = this.attackDamage;
        projectileComp.Launch(ignoreCollisions.SelectMany(go => go.GetComponentsInChildren<Collider>()).ToArray(), transform.forward, projectileSpeed);

        IEnumerator ResetProjectileDelayed(float secondsToWait)
        {
            yield return new WaitForSeconds(secondsToWait);
            if (projectileComp == null)
            {
                yield break;
            }

            Destroy(projectileComp);
            projectile.transform.SetParent(transform);
            projectile.transform.transform.position = this.attackPoint.position;
            projectile.gameObject.SetActive(false);
        }

        void OnCollided(Collision other)
        {
            Vector3 directionToTarget = (other.collider.transform.position - attackPoint.position).normalized;
            ApplyDamage(other.gameObject);
            ApplyKnockback(other.collider, directionToTarget * -1f); // invert knockback to "pull" towards player

            StartCoroutine(ResetProjectileDelayed(projectileLifetimeAfterValidCollision));
        }
        projectileComp.Collided += OnCollided;

        StartCoroutine(ResetProjectileDelayed(projectileLifetimeInSeconds));
    }
}