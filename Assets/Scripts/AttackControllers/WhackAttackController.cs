using UnityEngine;
using System.Collections;
using System.Linq;

public class WhackAttackController : AttackController
{
    [Header("Whack Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileFireDelayInSeconds = 0.5f;
    [SerializeField] private float projectileLifetimeInSeconds = 5f;

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
        var projectile = Instantiate(projectilePrefab).GetComponent<WhackProjectile>();
        
        projectile.gameObject.transform.position = this.attackPoint.position;
        projectile.Damage = this.attackDamage;
        projectile.Launch(ignoreCollisions.SelectMany(go => go.GetComponentsInChildren<Collider>()).ToArray(), transform.forward, projectileSpeed);

        IEnumerator DestroyProjectileDelayed()
        {
            yield return new WaitForSeconds(projectileLifetimeInSeconds);
            Destroy(projectile.gameObject);
        }

        StartCoroutine(DestroyProjectileDelayed());
    }
}