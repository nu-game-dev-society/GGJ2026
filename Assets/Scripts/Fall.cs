using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fall : MonoBehaviour
{
    private AnimationCurve curve;
    private Vector3 start;
    private Vector3 end;
    private float time;

    private float totalTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;

        float smoothed = curve.Evaluate(totalTime / time);
        transform.position = Vector3.Lerp(start, end, smoothed);

        // If we are finished then remove ourself
        if (smoothed >= 1f)
        {
            Destroy(this);
        }
    }

    internal void Setup(AnimationCurve curve, Vector3 start, Vector3 end, float time)
    {
        this.curve = curve;
        this.start = start;
        this.end = end;
        this.time = time;
        this.totalTime = 0;
    }
}
