using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public AudioSource audioSource;
    public AudioClip clip;

    private void OnTriggerEnter(Collider other)
    {
        // Check if a player entered the death zone
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Kill();

            if (particleSystem)
                particleSystem.Play();

            if (audioSource && clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }

        }
    }
}
