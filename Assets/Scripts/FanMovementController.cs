using UnityEngine;

public class FanMovementController : MonoBehaviour
{
    [SerializeField] private Transform upTransform;
    [SerializeField] private Transform downTransform;
    private Transform targetTransform;
    [SerializeField] private bool isUp;

    [Header("Whiplash")]
    [SerializeField] private float whiplashStrength = 0.3f;
    [SerializeField] private float whiplashSpeed = 20f;
    [SerializeField] private float rotationWhiplashStrength = 10f;

    private bool whiplashing;
    private bool finishedMostRecentMove;
    private float whipTime;
    private float lerpSpeed = 5f;

    private Vector3 whipDirection;
    private Vector3 rotationWhipAxis;

    private Transform lastTargetTransform;
    private bool hasMovedTowardTarget;

    public void SetIsUp(bool isUp)
    {
        this.isUp = isUp;
    }

    void Update()
    {
        targetTransform = isUp ? upTransform : downTransform;

        if (targetTransform != lastTargetTransform)
        {
            whiplashing = false;
            hasMovedTowardTarget = false;
            lastTargetTransform = targetTransform;
        }

        if (!whiplashing)
        {
            UpdateMoveTowardTarget();
        }
        else
        {
            UpdateWhiplashBeyondTarget();
        }
    }


    private void UpdateMoveTowardTarget()
    {
        float dist = Vector3.Distance(transform.position, targetTransform.position);

        if (dist > 0.01f)
            hasMovedTowardTarget = true;

        transform.position = Vector3.Lerp(
            transform.position,
            targetTransform.position,
            lerpSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetTransform.rotation,
            lerpSpeed * Time.deltaTime
        );

        // Only whiplash if we actually traveled toward this target
        if (hasMovedTowardTarget &&
            dist < 0.05f &&
            Quaternion.Angle(transform.rotation, targetTransform.rotation) < 2f)
        {
            whiplashing = true;
            whipTime = 0f;

            whipDirection = (transform.position - targetTransform.position).normalized;

            rotationWhipAxis = Vector3.Cross(
                transform.forward,
                targetTransform.forward
            ).normalized;
        }
    }

    private void UpdateWhiplashBeyondTarget()
    {
        whipTime += Time.deltaTime * whiplashSpeed;

        float damp = Mathf.Exp(-whipTime);
        float sine = Mathf.Sin(whipTime);

        transform.position =
            targetTransform.position +
            whipDirection * sine * whiplashStrength * damp;

        float rotOffset = sine * rotationWhiplashStrength * damp;
        transform.rotation =
            targetTransform.rotation *
            Quaternion.AngleAxis(rotOffset, rotationWhipAxis);

        // Settle
        if (whipTime > 2f)
        {
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            whiplashing = false;
            finishedMostRecentMove = true;
        }
    }
}
