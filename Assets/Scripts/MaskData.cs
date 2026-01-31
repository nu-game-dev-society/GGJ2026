using UnityEngine;

[CreateAssetMenu(fileName = "New Mask", menuName = "Game/Mask Data")]
public class MaskData : ScriptableObject
{
    [Header("Visual")]
    public string maskName;
    [TextArea(2, 4)]
    public string description;
    public GameObject maskPrefab; // The 3D model to attach to player's face
    public Vector3 attachOffset = Vector3.zero;
    public Vector3 attachRotation = Vector3.zero;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float healthRegenRate = 10f;
    public float healthRegenWaitTime = 2.0f;
    public float movementSpeedMultiplier = 1f;
    public float attackDamageMultiplier = 1f;
    public float knockbackResistance = 0f; // 0 = normal, 1 = immune to knockback
    public float attackCooldownMultiplier = 1f;

    [Header("Special Abilities")]
    public bool canDoubleJump = false;
    public bool canDash = false;
    public float specialAbilityCooldown = 5f;

    [Header("Visual Effects")]
    public Color glowColor = Color.white;
    public ParticleSystem equippedParticles; // Optional particle effect when worn
}
