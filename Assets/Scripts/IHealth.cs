using UnityEngine;

public interface IHealth
{
    void TakeDamage(float damage);
    void Heal(float amount);
    void SetHealthProperties(float? maxHealth = null, float? regenRate = null, float? regenWaitTime = null);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsAlive();
}
