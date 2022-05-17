using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    private float tumble;
    [SerializeField] private float maxTumble = 20;

    void Start()
    {
        SetTumble();
        GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble;
    }

    private void SetTumble()
    {
        tumble = Random.Range(maxTumble, 2 * maxTumble);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Peep"))
        {
            KillPeep(other.gameObject);
        }

        if (other.gameObject.CompareTag("Stage"))
        {
            DestroyAsteroid();
        }
    }

   

    private void DestroyAsteroid()
    {
        Destroy(this.gameObject);
    }

    private void KillPeep(GameObject peepToKill)
    {
        Destroy(peepToKill);
    }
}