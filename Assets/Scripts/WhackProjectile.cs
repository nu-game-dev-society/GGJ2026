using UnityEngine;
using System;
using System.Collections.Generic;

public class WhackProjectile : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpwardForce = 2f;
    [SerializeField] protected float characterControllerKnockbackMultiplier = 5f;

    public event Action<Collision> Collided;

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

        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }
        rigidBody.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == gameObject) return;

        Collided?.Invoke(other);
    }
}