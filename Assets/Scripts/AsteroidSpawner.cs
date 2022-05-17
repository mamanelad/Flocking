using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    private GameObject[] Peeps;
    [SerializeField] private float timeToSpawnTarget = 3f;
    private float timerTarget;
    [SerializeField] private List<GameObject> asteroids;
    [SerializeField] private GameObject asteroidCloneFather;
    [SerializeField] private GameObject _target;
    private Vector3 currTargetPosition;

    private bool spawnAsteroid;
    [SerializeField] private float timeDifferenceTargetAsteroid  = .3f;
    private float timerAsteroid;

    // Update is called once per frame
    void Update()
    {
        timerTarget -= Time.deltaTime;
        if (timerTarget <= 0)
        {
            timerTarget = timeToSpawnTarget;
            SpawnTarget();
        }

        if (spawnAsteroid)
        {
            timerAsteroid -= Time.deltaTime;
            if (timerAsteroid <= 0)
            {
                SpawnAsteroid();
            }
        }
        
        
    }


    private void SpawnTarget()
    {
        PositionToSpawnTarget();
        Instantiate(_target, currTargetPosition, Quaternion.identity);
        spawnAsteroid = true;
        timerAsteroid = timeDifferenceTargetAsteroid;
    }
    
    private void SpawnAsteroid()
    {
        var index = Random.Range(0, asteroids.Capacity);
        var currAsteroid = asteroids[index];
        var pos = transform.position;
        var newAstro = Instantiate(currAsteroid, pos, quaternion.identity);
        newAstro.transform.SetParent(asteroidCloneFather.transform);
        
        var direction = currTargetPosition - gameObject.transform.position;
        newAstro.GetComponent<Rigidbody>().velocity = 1 * direction;
        spawnAsteroid = false;
    }

    private void PositionToSpawnTarget()
    {
        currTargetPosition = Vector3.zero;
        var counter = 0f;
        Peeps = GameObject.FindGameObjectsWithTag("Peep");

        foreach (var peep in Peeps)
        {
            currTargetPosition += peep.transform.position;
            counter += 1;
        }

        if (counter ==0)
        {
            currTargetPosition = Vector3.zero;
            return;
        }


        currTargetPosition = new Vector3(currTargetPosition.x / counter, 0.01f, currTargetPosition.z / counter);
    }
}