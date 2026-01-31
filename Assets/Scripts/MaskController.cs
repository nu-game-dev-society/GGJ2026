using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Mesh;

public class MaskController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform maskModelsParent;

    [Header("Current Mask")]
    [SerializeField] private MaskData currentMask;

    [Header("Events")]
    public UnityEvent<MaskData> OnMaskEquipped;
    public UnityEvent OnMaskRemoved;

    private IHealth healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<IHealth>();
    }

    private void EquipMask(MaskData maskData)
    {
        if (maskData == null) return;

        // Remove current mask if any
        RemoveMask();

        // Set new mask
        currentMask = maskData;

        // Make all needed models active
        foreach (string modelName in maskData.modelNames)
        {
            Transform modelTransform = maskModelsParent.Find(modelName);
            if (modelTransform != null)
            {
                modelTransform.gameObject.SetActive(true);
            }
        }

        // Apply mask properties
        ApplyMaskProperties();

        OnMaskEquipped?.Invoke(maskData);

        // ryan needs to connect this to make it compile
        // playerController.OnMaskEquipped(maskData.AttackType);
    }

    public void RemoveMask()
    {
        if (currentMask == null) return;

        // Remove mask properties before destroying
        RemoveMaskProperties();

        // Make all needed models inactive
        foreach (string modelName in currentMask.modelNames)
        {
            Transform modelTransform = maskModelsParent.Find(modelName);
            if (modelTransform != null)
            {
                modelTransform.gameObject.SetActive(false);
            }
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
