using System.Collections.Generic;
using Flocking;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    #region Private Fields

    private float astroCurrScale = 1f;
    private float timerTarget;
    private float timerAsteroid;

    private int whenToHitPlayerCounter;

    private bool spawnAsteroid;

    private GameObject[] peeps;
    private GameObject player;

    private Vector3 currTargetPosition;

    #endregion

    #region Public Fields

    public bool spawn = true;
    
    #endregion
    
    #region Inspector Control
    
    [Header("Asteroid speed Settings")]
    
    [SerializeField] private float maxSpeed = 35f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float speedStep = 0.05f;
    
    [Header("Asteroid Scale Settings")]

    [SerializeField] private float astroScaleStep = .01f;
    
    [Header("Timing Settings")] 
    
    [SerializeField] private List<GameObject> asteroids;
    [SerializeField] private GameObject asteroidCloneFather;
    [SerializeField] private GameObject target;
    [SerializeField] private float timeDifferenceTargetAsteroid = .3f;
    [SerializeField] private int whenHitPlayerRoutine = 3;
    [SerializeField] private int whenToHitPlayerHarder = 10;
    
    [Header("Needed Game objects Settings")] 

    [SerializeField] private float timeToSpawnTarget = 3f;
    [SerializeField] private float targetTimeStep = 0.05f;
    [SerializeField] private float minTimeToSpawnTarget = 1f;
    
    #endregion
    
    private void Awake()
    {
        player = GameObject.FindObjectOfType<PlayerPeepController>().gameObject;
    }

    private void Update()
    {
        if (!spawn) return;
        timerTarget -= Time.deltaTime;
        if (timerTarget <= 0)
        {
            timerTarget = timeToSpawnTarget;
            if (timeToSpawnTarget > minTimeToSpawnTarget)
            {
                timeToSpawnTarget -= targetTimeStep;
            }

            SpawnTarget();
        }

        if (!spawnAsteroid) return;
        timerAsteroid -= Time.deltaTime;
        if (timerAsteroid <= 0)
        {
            SpawnAsteroid();
        }
    }

    /**
     * Initialize a new target
     */
    private void SpawnTarget()
    {
        PositionToSpawnTarget();
        Instantiate(target, currTargetPosition, Quaternion.identity);
        spawnAsteroid = true;
        timerAsteroid = timeDifferenceTargetAsteroid;
    }

    /**
     * Initialize a new astride.
     */
    private void SpawnAsteroid()
    {
        var index = Random.Range(0, asteroids.Capacity);
        var currAsteroid = asteroids[index];
        var pos = transform.position;
        var newAstro = Instantiate(currAsteroid, pos, quaternion.identity);
        newAstro.transform.localScale = new Vector3(astroCurrScale, astroCurrScale, astroCurrScale);
        astroCurrScale += astroScaleStep;
        newAstro.transform.SetParent(asteroidCloneFather.transform);

        var direction = (currTargetPosition - gameObject.transform.position).normalized;

        newAstro.GetComponent<Rigidbody>().velocity = speed * direction;
        if (speed < maxSpeed)
        {
            speed += speedStep;
        }

        spawnAsteroid = false;
    }

    /**
     * Calculate the new position for the next target.
     * The average of all peeps or at the player.
     */
    private void PositionToSpawnTarget()
    {
        whenToHitPlayerCounter += 1;
        if (whenToHitPlayerCounter % whenHitPlayerRoutine == 0)
        {
            currTargetPosition = player.transform.position;
            return;
        }

        if (whenHitPlayerRoutine > 1 && whenToHitPlayerCounter == whenToHitPlayerHarder)
        {
            whenToHitPlayerCounter = 0;
            whenHitPlayerRoutine -= 1;
        }

        currTargetPosition = Vector3.zero;
        var counter = 0f;
        peeps = GameObject.FindGameObjectsWithTag("Peep");

        foreach (var peep in peeps)
        {
            currTargetPosition += peep.transform.position;
            counter += 1;
        }

        if (counter == 0)
        {
            currTargetPosition = Vector3.zero;
            return;
        }


        currTargetPosition = new Vector3(currTargetPosition.x / counter, 0.1f, currTargetPosition.z / counter);
    }
}