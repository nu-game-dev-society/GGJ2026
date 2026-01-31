using UnityEngine;

public class MeatGrinderTrap : MonoBehaviour
{
    [field: SerializeField]
    public Transform[] Rotators { get; set; }
    [field: SerializeField]
    public float RotateSpeed { get; set; } = 10.0f;
    [field: SerializeField]
    public Vector3 RollDirection { get; set; }

    void Update()
    {
        for (int i = 0; i < Rotators.Length; i++)
        {
            Rotators[i].Rotate(RollDirection, RotateSpeed * Time.deltaTime);
        }

    }
}
