using UnityEngine;

public class PlayParticle : MonoBehaviour
{
    public void PlayParticleEvent(ParticleSystem particle)
    {
        particle.Play();
    }
}
