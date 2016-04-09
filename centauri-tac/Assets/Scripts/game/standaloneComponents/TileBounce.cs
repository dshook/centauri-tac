using System;
using UnityEngine;

public class TileBounce : MonoBehaviour
{
    public float offset = 0f;
    public float delay = 0f;
    public float magnitudeMult = 1f;
    public float waveDuration = 1.5f;

    private float waveMagnitude = 0.5f;
    private float waveFrequency = 0.030f;
    private float timeAcc = 0f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        timeAcc += Time.deltaTime;

        if(timeAcc < delay) return;

        Func<float, float> waveFunc = (x) => 
            ((x / waveDuration * -1 + (waveMagnitude)) * ((float)Math.Sin((x - offset) / waveFrequency) * 0.5f * magnitudeMult));

        transform.position = originalPosition + new Vector3(0, waveFunc(timeAcc), 0);

        if (timeAcc + delay > waveMagnitude * waveDuration)
        {
            transform.position = originalPosition;
            Destroy(this);
        }

    }
}
