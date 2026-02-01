using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanManager : MonoBehaviour
{
    [SerializeField] private List<FanMovementController> fans;
    [SerializeField] private float intialDowntime = 20f;
    [SerializeField] private float upTime;
    [SerializeField] private float downTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoopUpAndDown());
    }
    
    IEnumerator LoopUpAndDown()
    {
        SetAllIsUp(false);

        yield return new WaitForSeconds(intialDowntime);

        do
        {
            SetAllIsUp(false);

            yield return new WaitForSeconds(downTime);

            fans[Random.Range(0, fans.Count)].SetIsUp(true);

            yield return new WaitForSeconds(upTime);
        }
        while (true);
    }

    private void SetAllIsUp(bool isUp)
    {
        foreach (var fan in fans)
        {
            fan.gameObject.SetActive(true); // just to be safe
            fan.SetIsUp(isUp);
        }
    }
}
