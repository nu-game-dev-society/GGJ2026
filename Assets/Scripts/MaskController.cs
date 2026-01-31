using UnityEngine;
using UnityEngine.Events;

public class MaskController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform maskAttachPoint; // Where the mask attaches to the player's face

    [Header("Current Mask")]
    [SerializeField] private MaskData currentMask;
    private GameObject currentMaskObject;

    [Header("Events")]
    public UnityEvent<MaskData> OnMaskEquipped;
    public UnityEvent OnMaskRemoved;

    private IHealth healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<IHealth>();
    }

    public void EquipMask(MaskData maskData)
    {
        if (maskData == null) return;

        // Remove current mask if any
        RemoveMask();

        // Set new mask
        currentMask = maskData;

        // Instantiate mask prefab
        if (maskData.maskPrefab != null && maskAttachPoint != null)
        {
            currentMaskObject = Instantiate(maskData.maskPrefab, maskAttachPoint);
            currentMaskObject.transform.localPosition = maskData.attachOffset;
            currentMaskObject.transform.localRotation = Quaternion.Euler(maskData.attachRotation);
        }

        // Apply mask properties
        ApplyMaskProperties();

        OnMaskEquipped?.Invoke(maskData);
    }

    public void RemoveMask()
    {
        if (currentMask == null) return;

        // Remove mask properties before destroying
        RemoveMaskProperties();

        // Destroy mask object
        if (currentMaskObject != null)
        {
            Destroy(currentMaskObject);
            currentMaskObject = null;
        }

        currentMask = null;
        OnMaskRemoved?.Invoke();
    }

    private void ApplyMaskProperties()
    {
        if (currentMask == null) return;

        // Apply max health
        if (healthComponent != null)
        {
            healthComponent.SetHealthProperties(currentMask.maxHealth, currentMask.healthRegenRate, currentMask.healthRegenWaitTime);
        }

        // Apply movement speed modifier
        var movement = GetComponent<IMovement>();
        if (movement != null)
        {
            movement.SetSpeedModifier(currentMask.movementSpeedMultiplier);
        }

        // Apply attack damage modifier
        var attackController = GetComponent<AttackController>();
        if (attackController != null)
        {
            attackController.SetDamageModifier(currentMask.attackDamageMultiplier);
        }
    }

    private void RemoveMaskProperties()
    {
        if (currentMask == null) return;

        // Reset to default values
        if (healthComponent != null)
        {
            healthComponent.SetHealthProperties(100f); // Default health
        }

        var movement = GetComponent<IMovement>();
        if (movement != null)
        {
            movement.SetSpeedModifier(1f);
        }

        var attackController = GetComponent<AttackController>();
        if (attackController != null)
        {
            attackController.SetDamageModifier(1f);
        }
    }

    public MaskData GetCurrentMask()
    {
        return currentMask;
    }

    public bool HasMask()
    {
        return currentMask != null;
    }

    // Called when player picks up a mask
    private void OnTriggerEnter(Collider other)
    {
        MaskPickup maskPickup = other.GetComponent<MaskPickup>();
        if (maskPickup != null)
        {
            EquipMask(maskPickup.maskData);
            Destroy(other.gameObject); // Remove pickup from world
        }
    }
}

// Simple interface for movement controllers
public interface IMovement
{
    void SetSpeedModifier(float modifier);
}
