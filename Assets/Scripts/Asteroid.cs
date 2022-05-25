using System;
using UnityEngine;
using System.Collections;
using Flocking;
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
    private MeshRenderer meshRenderer;


     private GameManager gameManager;

    void Start()
    {
        SetTumble();
        GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * tumble;
        gameManager = FindObjectOfType<GameManager>();
        meshRenderer = GetComponent<MeshRenderer>();
        FindObjectOfType<AudioManager>().PlaySound("Fall");
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
        if (other.gameObject.CompareTag("Player"))
        {
            // KillPeep(other.gameObject);
            gameManager.GameFinish(false);

        }

        
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
        FindObjectOfType<AudioManager>().PlaySound("Hit");
        meshRenderer.enabled = false;
        var transform1 = transform;
        currExplosion = Instantiate(explosionEffect, transform1.position, transform1.rotation);
        
    }

    private void DestroyAsteroid()
    {
        Destroy(currExplosion);
        Destroy(gameObject);
    }

    private void KillPeep(GameObject peepToKill)
    {
        FindObjectOfType<AudioManager>().PlaySound("Scream");
        Destroy(peepToKill.GetComponentInParent<FollowerPeepController>().gameObject);
        gameManager.numPlayersTotal -= 1;

    }
}