using UnityEngine;
using UnityEngine.Events;

public class DestructibleHealthComponent : MonoBehaviour, IHealth
{

    //todo: base on masks?
    [field: SerializeField]
    public float MaxHealth { get; set; } = 10.0f;


    [field: SerializeField]
    public float Health { get; private set; }

    [field: SerializeField]
    public UnityEvent OnDeath { get; set; }


    public void Start()
    {
        Health = MaxHealth;
    }


    public void TakeDamage(float damage)
    {
        Health -= damage;
        if(Health <= 0)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
    }

    public void SetHealthProperties(float? maxHealth = null, float? regenRate = null, float? regenWaitTime = null)
    {
        float healthPercentage = Health / MaxHealth;
        if (maxHealth != null)
            MaxHealth = maxHealth.Value;
        Health = MaxHealth * healthPercentage;
    }

    public float GetCurrentHealth()
    {
        return Health;
    }

    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    public bool IsAlive()
    {
        return Health > 0;
    }

}
