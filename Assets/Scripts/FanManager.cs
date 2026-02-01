using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanManager : MonoBehaviour
{
    [SerializeField] private List<FanMovementController> fans;
    [SerializeField] private float upTime;
    [SerializeField] private float downTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoopUpAndDown());
    }
    
    IEnumerator LoopUpAndDown()
    {
        do
        {
            foreach (var fan in fans)
            {
                fan.gameObject.SetActive(true);
                fan.SetIsUp(false);
            }

            yield return new WaitForSeconds(downTime);

            fans[Random.Range(0, fans.Count)].SetIsUp(true);

            yield return new WaitForSeconds(upTime);
        }
        while (true);
    }
}
