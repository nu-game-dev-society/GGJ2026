using UnityEngine;

public class MeatGrinderTrap : MonoBehaviour
{
    [field: SerializeField]
    public Transform[] Rotators { get; set; }
    [field: SerializeField]
    public float RotateSpeed { get; set; } = 10.0f;

    void Update()
    {
        for (int i = 0; i < Rotators.Length; i++)
        {
            Rotators[i].Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
        }

    }
}
