using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public AudioSource audioSource;
    public AudioClip clip;

    [SerializeField] private ParticleSystem playerDeathParticles;
    public float yPosition; 

    private void OnTriggerEnter(Collider other)
    {
        // Check if a player entered the death zone
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (playerDeathParticles != null)
            {
                Vector3 position = other.transform.position; 
                position.y = yPosition; 
                ParticleSystem particles = Instantiate(playerDeathParticles, position, Quaternion.identity);
            }

            player.Kill();

            if (particleSystem)
                particleSystem.Play();

            if (audioSource && clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }

        }
        else
        {
            var health = other.GetComponent<IHealth>();
            health?.TakeDamage(2134567890);
        }
    }
}
