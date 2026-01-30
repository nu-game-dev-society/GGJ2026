
using UnityEngine;

public class PlayerHealthController : MonoBehaviour, IHealth
{

    //todo: base on masks?
    [field: SerializeField]
    public float MaxHealth { get; set; } = 10.0f;
    [field: SerializeField]
    public float RegenTime { get; set; } = 5.0f;
    [field: SerializeField]
    public float RegenRate { get; set; } = 0.5f;

    [field: SerializeField]
    public float Health { get; private set; }
    [field: SerializeField]
    public float LastHitReceivedTime { get; private set; }

    public void Start()
    {
        Health = MaxHealth;
    }

    public void Update()
    {
        if (LastHitReceivedTime + RegenTime < Time.time)
        {
            Health = Mathf.Min(Health + (RegenRate * Time.deltaTime), MaxHealth);
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
        LastHitReceivedTime = Time.time;
    }
}