using UnityEngine;

public class RollerPusher : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private Vector3 pushDirection = Vector3.right; // Direction to push (in local space)

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
        // Get rotation speed from MeatGrinderTrap if available
        float rotationSpeed = meatGrinder != null ? meatGrinder.RotateSpeed : 10f;

        // Calculate push direction based on rotation
        Vector3 worldPushDirection = CalculatePushDirection(contactPoint, rotationSpeed);

        return worldPushDirection * pushForce;
    }

    private Vector3 CalculatePushDirection(Vector3 contactPoint, float rotationSpeed)
    {
        // If rotation speed is positive, use the push direction as-is
        // If negative, reverse it
        Vector3 direction = transform.TransformDirection(pushDirection);

        if (rotationSpeed < 0)
        {
            direction = -direction;
        }

        return direction.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw push direction in editor
        Gizmos.color = Color.red;
        Vector3 worldDir = transform.TransformDirection(pushDirection);
        Gizmos.DrawRay(transform.position, worldDir * 2f);
        Gizmos.DrawSphere(transform.position + worldDir * 2f, 0.2f);
    }
}
