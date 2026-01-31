using UnityEngine;

public class MaskPickup : MonoBehaviour
{
    [Header("Mask Data")]
    public MaskData maskData;

    [Header("Visuals")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private bool animatePickup = true;

    [SerializeField] private GameObject model;

    private Vector3 startPosition;
    private float offset;

    private void Start()
    {
        startPosition = model.transform.position;
        offset = Random.value * (Mathf.PI / 2);
    }

    private void Update()
    {
        if (!animatePickup) return;

        // Rotate the mask
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed + offset) * bobHeight;
        model.transform.position = new Vector3(model.transform.position.x, newY, model.transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // This is handled by MaskController, but you can add pickup effects here
        if (other.GetComponent<MaskController>() != null)
        {
            // Play pickup sound/effect
            PlayPickupEffect();
        }
    }

    private void PlayPickupEffect()
    {
        // Add particle effects, sounds, etc.
    }
}
