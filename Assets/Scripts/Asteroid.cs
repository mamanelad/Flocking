using System;
using UnityEngine;
using System.Collections;
using Object = System.Object;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    private float tumble;
    [SerializeField] private float maxTumble = 20;
    [SerializeField] private float timeToDestroy = 0.5f;
    [SerializeField] private GameObject explosionEffect;
    private bool hadExplode;
    private GameObject currExplosion;


    void Start()
    {
        SetTumble();
        GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble;
    }

    private void Update()
    {
        if (hadExplode)
        {
            timeToDestroy -= Time.deltaTime;
            if (timeToDestroy <= 0)
            {
                DestroyAsteroid();
            }
        }
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

        if (other.gameObject.CompareTag("Target") && !hadExplode)
        {
            hadExplode = true;
            Explode();
            Destroy(other.gameObject);
        }
        
    }

    private void Explode()
    {
        var transform1 = transform;
        currExplosion = Instantiate(explosionEffect, transform1.position, transform1.rotation);
        
    }

    private void DestroyAsteroid()
    {
        Destroy(currExplosion);
        Destroy(this.gameObject);
    }

    private void KillPeep(GameObject peepToKill)
    {
        Destroy(peepToKill);
    }
}