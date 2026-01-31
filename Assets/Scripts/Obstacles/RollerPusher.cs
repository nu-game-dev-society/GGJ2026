using UnityEngine;

public class RollerPusher : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float upwardForce = 0.5f; // Helps keep player on top

    [Header("Rotation Reference")]
    [SerializeField] private Transform rotatingTransform; // The visual roller that's rotating

    private MeatGrinderTrap meatGrinder;

    private void Start()
    {
        // Try to get MeatGrinderTrap from parent or this object
        meatGrinder = GetComponentInParent<MeatGrinderTrap>();
        if (meatGrinder == null)
        {
            meatGrinder = GetComponent<MeatGrinderTrap>();
        }

        // If no rotating transform specified, use this transform
        if (rotatingTransform == null)
        {
            rotatingTransform = transform;
        }
    }

    /// <summary>
    /// Called by PlayerController when it collides with this roller
    /// </summary>
    public Vector3 GetPushForce(Vector3 contactPoint)
    {
        Vector3 pushDirection = GetPushDirection(contactPoint);
        return pushDirection * pushForce;
    }

    private Vector3 GetPushDirection(Vector3 contactPoint)
    {
        // Get rotation speed from MeatGrinderTrap if available
        float rotationSpeed = meatGrinder != null ? meatGrinder.RotateSpeed : 10f;

        // Calculate the tangent direction (perpendicular to radius)
        // Since the roller rotates around Z-axis (forward), we calculate tangent in XY plane
        Vector3 toContact = contactPoint - rotatingTransform.position;
        toContact.z = 0; // Flatten to XY plane

        // Tangent is perpendicular to radius (cross product with rotation axis)
        Vector3 rotationAxis = rotatingTransform.forward * Mathf.Sign(rotationSpeed);
        Vector3 pushDirection = Vector3.Cross(rotationAxis, toContact.normalized);

        // Add slight upward force to keep player on top of roller
        pushDirection.y += upwardForce;

        return pushDirection.normalized;
    }
}
