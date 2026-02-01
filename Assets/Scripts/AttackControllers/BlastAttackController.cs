using UnityEngine;
using System.Collections;
using System.Linq;

public class BlastAttackController : AttackController
{
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Blast Projectile")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileFireDelayInSeconds = 0.5f;
    [SerializeField] private float projectileLifetimeInSeconds = 5f;
    [SerializeField] private float projectileLifetimeAfterValidCollision = 1f;

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

        projectile.gameObject.SetActive(true);
        projectile.transform.SetParent(null);

        var projectileComp = projectile.AddComponent<WhackProjectile>();
        projectileComp.gameObject.transform.position = this.attackPoint.position;
        projectileComp.gameObject.transform.rotation = Quaternion.LookRotation(transform.forward*-1f);// idk why we need to flip this but we do
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