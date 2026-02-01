
using UnityEngine;
using UnityEngine.Events;

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
    public float StunDuration { get; set; } = 3.0f; // How long to stun when health reaches 0

    [Header("Events")]
    [SerializeField] private UnityEvent<float> onHealthDepleted; // Passes stun duration
    [SerializeField] private UnityEvent onTakeDamage;

    [field: SerializeField]
    public float Health { get; set; }
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

        onTakeDamage?.Invoke();

        if (Health <= 0)
        {
            // Stun the player when health reaches 0
            Health = 0; // Clamp to 0
            onHealthDepleted?.Invoke(StunDuration);
        }
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
    }

    public void SetHealthProperties(float? maxHealth = null, float? regenRate = null, float? regenWaitTime = null)
    {
        float healthPercentage = Health / MaxHealth;
        if(maxHealth != null)
            MaxHealth = maxHealth.Value;
        if(regenRate != null)
            RegenRate = regenRate.Value;
        if(regenWaitTime != null)
            RegenTime = regenRate.Value;
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