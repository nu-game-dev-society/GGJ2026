using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanController : MonoBehaviour
{
    [Header("Fan Settings")]
    [SerializeField] private float fanForce = 15f;
    [SerializeField] private float maxFloatHeight = 10f; // Max height above fan before force starts decreasing
    [SerializeField] private bool startOn = false;

    [Header("Random Intervals")]
    [SerializeField] private float minOnTime = 3f;
    [SerializeField] private float maxOnTime = 8f;
    [SerializeField] private float minOffTime = 2f;
    [SerializeField] private float maxOffTime = 6f;

    [Header("Character Controller Settings")]
    [SerializeField] private float characterMovementMultiplier = 0.3f; // Movement speed when in fan

    [Header("Visual/Audio")]
    [SerializeField] private Transform fanBladesTransform; // The visual blades to rotate
    [SerializeField] private float fanRotationSpeed = 720f; // Degrees per second when on
    [SerializeField] private float rotationAcceleration = 360f; // How fast it speeds up/slows down
    [SerializeField] private ParticleSystem fanParticles;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private bool isOn;
    private float currentRotationSpeed = 0f;
    private HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();
    private HashSet<CharacterController> affectedCharacterControllers = new HashSet<CharacterController>();
    private Dictionary<CharacterController, IMovement> characterMovements = new Dictionary<CharacterController, IMovement>();

    private void Start()
    {
        isOn = startOn;
        UpdateFanVisuals();
        StartCoroutine(RandomIntervalRoutine());
    }

    private void FixedUpdate()
    {
        if (!isOn) return;

        // Apply force to rigidbodies
        foreach (Rigidbody rb in affectedRigidbodies)
        {
            if (rb == null) continue;

            // Calculate force based on distance from fan
            float distanceFromFan = Vector3.Distance(transform.position, rb.position);
            float forceFalloff = Mathf.Clamp01(1f - (distanceFromFan / maxFloatHeight));
            float currentForce = fanForce * forceFalloff;

            // Apply upward force - momentum is automatically conserved
            rb.AddForce(Vector3.up * currentForce, ForceMode.Force);
        }
    }

    private void Update()
    {
        // Smoothly interpolate rotation speed
        float targetSpeed = isOn ? fanRotationSpeed : 0f;
        currentRotationSpeed = Mathf.MoveTowards(currentRotationSpeed, targetSpeed, rotationAcceleration * Time.deltaTime);

        // Rotate fan blades
        if (fanBladesTransform != null)
        {
            fanBladesTransform.Rotate(Vector3.up, currentRotationSpeed * Time.deltaTime, Space.Self);
        }

        if (!isOn) return;

        // Handle character controllers
        foreach (CharacterController cc in affectedCharacterControllers)
        {
            if (cc == null) continue;

            // Apply upward movement to character controller
            float distanceFromFan = Vector3.Distance(transform.position, cc.transform.position);
            float forceFalloff = Mathf.Clamp01(1f - (distanceFromFan / maxFloatHeight));
            float upwardSpeed = fanForce * forceFalloff * Time.deltaTime;

            // Move character upward (CharacterController.Move adds to existing velocity)
            cc.Move(Vector3.up * upwardSpeed);
        }
    }

    private IEnumerator RandomIntervalRoutine()
    {
        while (true)
        {
            if (isOn)
            {
                // Fan is on, wait for random duration then turn off
                float onDuration = Random.Range(minOnTime, maxOnTime);
                yield return new WaitForSeconds(onDuration);
                SetFanState(false);
            }
            else
            {
                // Fan is off, wait for random duration then turn on
                float offDuration = Random.Range(minOffTime, maxOffTime);
                yield return new WaitForSeconds(offDuration);
                SetFanState(true);
            }
        }
    }

    private void SetFanState(bool state)
    {
        isOn = state;
        UpdateFanVisuals();

        if (!isOn)
        {
            // Remove movement modifiers when fan turns off
            RestoreCharacterMovement();
        }
        else
        {
            // Apply movement modifiers when fan turns on
            ApplyCharacterMovement();
        }
    }

    private void UpdateFanVisuals()
    {
        // Update particles
        if (fanParticles != null)
        {
            if (isOn)
                fanParticles.Play();
            else
                fanParticles.Stop();
        }

        // Update audio
        if (audioSource != null)
        {
            if (isOn)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Track rigidbodies
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            affectedRigidbodies.Add(rb);
            return;
        }

        // Track character controllers
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            affectedCharacterControllers.Add(cc);

            // Get movement component if exists
            IMovement movement = other.GetComponent<IMovement>();
            if (movement != null)
            {
                characterMovements[cc] = movement;
                if (isOn)
                {
                    movement.SetSpeedModifier(characterMovementMultiplier);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove rigidbodies
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            affectedRigidbodies.Remove(rb);
            return;
        }

        // Remove character controllers and restore movement
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            affectedCharacterControllers.Remove(cc);

            if (characterMovements.TryGetValue(cc, out IMovement movement))
            {
                movement.SetSpeedModifier(1f);
                characterMovements.Remove(cc);
            }
        }
    }

    private void ApplyCharacterMovement()
    {
        foreach (var kvp in characterMovements)
        {
            kvp.Value.SetSpeedModifier(characterMovementMultiplier);
        }
    }

    private void RestoreCharacterMovement()
    {
        foreach (var kvp in characterMovements)
        {
            kvp.Value.SetSpeedModifier(1f);
        }
    }

    public bool IsOn()
    {
        return isOn;
    }

    [ContextMenu("Toggle Fan")]
    public void ToggleFan()
    {
        SetFanState(!isOn);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw max float height
        Gizmos.color = isOn ? Color.cyan : Color.gray;
        Vector3 topPosition = transform.position + Vector3.up * maxFloatHeight;
        Gizmos.DrawLine(transform.position, topPosition);
        Gizmos.DrawWireSphere(topPosition, 0.5f);

        // Draw fan influence cone
        Gizmos.color = isOn ? new Color(0, 1, 1, 0.2f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);

        // Simple cone visualization
        Vector3[] conePoints = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            float radius = 1f;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            conePoints[i] = topPosition + offset;
            Gizmos.DrawLine(transform.position, conePoints[i]);
        }
    }

    private void OnDestroy()
    {
        // Clean up movement modifiers
        RestoreCharacterMovement();
    }
}
