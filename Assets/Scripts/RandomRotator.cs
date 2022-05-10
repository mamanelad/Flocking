using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class RandomRotator : MonoBehaviour
{
    private float tumble;

    void Start()
    {
        GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble;
    }

    public void SetTumble(float newTumble)
    {
        tumble = newTumble;
    }
}