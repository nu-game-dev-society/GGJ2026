using UnityEngine;

[CreateAssetMenu(fileName = "New Mask", menuName = "Game/Mask Data")]
public class MaskData : ScriptableObject
{
    [Header("Visual")]
    public string maskName;
    [TextArea(2, 4)]
    public string description;
    public string[] modelNames; // Names of 3D models associated with the mask
    public AttackType attackType;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float healthRegenRate = 10f;
    public float healthRegenWaitTime = 2.0f;
    public float movementSpeedMultiplier = 1f;
    public float attackDamageMultiplier = 1f;
    [Range(0f, 1f)]
    public float knockbackResistance = 0f; // 0 = normal, 1 = immune to knockback
    public float attackCooldownMultiplier = 1f;
}
