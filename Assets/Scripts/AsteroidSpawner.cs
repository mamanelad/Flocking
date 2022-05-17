using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float timeToSpawn = 1f;
    private float _timer;
    [SerializeField] private List<GameObject> asteroids;
    [SerializeField] private GameObject asteroidCloneFather;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
        {
            _timer = timeToSpawn;
            var index = Random.Range(0, asteroids.Capacity);
            var currAsteroid = asteroids[index];
            var pos = PositionToSpawn();
            var newAstro = Instantiate(currAsteroid, pos, quaternion.identity);
            newAstro.transform.SetParent(asteroidCloneFather.transform);
        }
        
    }

    private Vector3 PositionToSpawn()
    {
        return transform.position;
    }
}
