using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if a player entered the death zone
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Kill();
        }
    }
}
