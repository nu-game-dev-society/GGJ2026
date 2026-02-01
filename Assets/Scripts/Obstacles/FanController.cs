using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanController : MonoBehaviour
{
    [Header("Fan Settings")]
    [SerializeField] private float fanForce = 15f;
    [SerializeField] private float maxFloatHeight = 10f;
    [SerializeField] private bool startOn = false;

    [Header("Random Intervals")]
    [SerializeField] private float minOnTime = 3f;
    [SerializeField] private float maxOnTime = 8f;
    [SerializeField] private float minOffTime = 2f;
    [SerializeField] private float maxOffTime = 6f;

    [Header("Character Controller Settings")]
    [SerializeField] private float characterForceMultiplier = 0.1f;
    [SerializeField] private float characterMovementMultiplier = 0.3f;

    [Header("Visual / Audio")]
    [SerializeField] private Transform fanBladesTransform;
    [SerializeField] private float fanRotationSpeed = 720f;
    [SerializeField] private float rotationAcceleration = 360f;
    [SerializeField] private ParticleSystem fanParticles;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Fade")]
    [SerializeField] private float audioFadeDuration = 1f;
    [SerializeField] private float maxAudioVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    public Vector3 Direction;
    public Vector3 RotateDirection;

    private bool isOn;
    private float currentRotationSpeed = 0f;

    private Coroutine audioFadeRoutine;

    private HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();
    private HashSet<CharacterController> affectedCharacterControllers = new HashSet<CharacterController>();
    private Dictionary<CharacterController, IMovement> characterMovements = new Dictionary<CharacterController, IMovement>();

    private void Start()
    {
        isOn = startOn;

        if (audioSource != null)
        {
            audioSource.volume = isOn ? maxAudioVolume : 0f;
            if (isOn) audioSource.Play();
        }

        UpdateFanVisuals();
        StartCoroutine(RandomIntervalRoutine());
    }

    private void FixedUpdate()
    {
        if (!isOn) return;

        foreach (Rigidbody rb in affectedRigidbodies)
        {
            if (rb == null) continue;

            float distance = Vector3.Distance(transform.position, rb.position);
            float falloff = Mathf.Clamp01(1f - (distance / maxFloatHeight));
            rb.AddForce(Direction * fanForce * falloff, ForceMode.Force);
        }
    }

    private void Update()
    {
        float targetSpeed = isOn ? fanRotationSpeed : 0f;
        currentRotationSpeed = Mathf.MoveTowards(
            currentRotationSpeed,
            targetSpeed,
            rotationAcceleration * Time.deltaTime
        );

        if (fanBladesTransform != null)
        {
            fanBladesTransform.Rotate(
                RotateDirection,
                currentRotationSpeed * Time.deltaTime,
                Space.Self
            );
        }

        if (!isOn) return;

        foreach (CharacterController cc in affectedCharacterControllers)
        {
            if (cc == null) continue;

            float distance = Vector3.Distance(transform.position, cc.transform.position);
            float falloff = Mathf.Clamp01(1f - (distance / maxFloatHeight));

            float upwardSpeed =
                fanForce * characterForceMultiplier * falloff * Time.deltaTime;

            cc.Move(Direction * upwardSpeed);
        }
    }

    private IEnumerator RandomIntervalRoutine()
    {
        while (true)
        {
            float waitTime = isOn
                ? Random.Range(minOnTime, maxOnTime)
                : Random.Range(minOffTime, maxOffTime);

            yield return new WaitForSeconds(waitTime);
            SetFanState(!isOn);
        }
    }

    private void SetFanState(bool state)
    {
        isOn = state;
        UpdateFanVisuals();

        if (isOn)
            ApplyCharacterMovement();
        else
            RestoreCharacterMovement();
    }

    private void UpdateFanVisuals()
    {
        if (fanParticles != null)
        {
            if (isOn) fanParticles.Play();
            else fanParticles.Stop();
        }

        if (audioSource != null)
        {
            if (audioFadeRoutine != null)
                StopCoroutine(audioFadeRoutine);

            if (isOn)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();

                audioFadeRoutine = StartCoroutine(FadeAudio(maxAudioVolume));
            }
            else
            {
                audioFadeRoutine = StartCoroutine(FadeAudio(0f));
            }
        }
    }

    private IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < audioFadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                time / audioFadeDuration
            );
            yield return null;
        }
        audioSource.volume = targetVolume;

        if (Mathf.Approximately(targetVolume, 0f))
            audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            affectedRigidbodies.Add(rb);
            return;
        }

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            affectedCharacterControllers.Add(cc);

            IMovement movement = other.GetComponent<IMovement>();
            if (movement != null)
            {
                characterMovements[cc] = movement;
                if (isOn)
                    movement.SetSpeedModifier(characterMovementMultiplier);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            affectedRigidbodies.Remove(rb);
            return;
        }

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
            kvp.Value.SetSpeedModifier(characterMovementMultiplier);
    }

    private void RestoreCharacterMovement()
    {
        foreach (var kvp in characterMovements)
            kvp.Value.SetSpeedModifier(1f);
    }

    public bool IsOn() => isOn;

    [ContextMenu("Toggle Fan")]
    public void ToggleFan()
    {
        SetFanState(!isOn);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = isOn ? Color.cyan : Color.gray;
        Vector3 top = transform.position + Vector3.up * maxFloatHeight;
        Gizmos.DrawLine(transform.position, top);
        Gizmos.DrawWireSphere(top, 0.5f);
    }

    private void OnDestroy()
    {
        RestoreCharacterMovement();
    }
}
