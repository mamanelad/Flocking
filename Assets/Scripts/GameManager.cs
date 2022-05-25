using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avrahamy.Collections;
using Flocking;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [SerializeField] private bool gameWon;
    [SerializeField] private bool _playerIsDead;
    [SerializeField] private GameObject gameFinishScreen;
    [SerializeField] private Text gameFinishText;
    [SerializeField] private float numPlayersToWon = 10f;
    
    [SerializeField] private AsteroidSpawner asteroidSpawner;
    [SerializeField] private float peepInitializeSpeed = 0.05f;
    [SerializeField] private GameObject player; // for now will be red leader


    [SerializeField]
    private List<GameObject> prefabsGroups; // the prefabs of followers, one from each group - no player

    [SerializeField] private List<int> numPrefabsGroups; // how much followers we want from each group


    [SerializeField] private float width; // width (=height) of the stage
    [SerializeField] public int numPlayersTotal; // including the player


    private List<GameObject> peeps = new List<GameObject>();
    private int numPlayersInstantiated = 0;
    private bool _initiating = true;

    [SerializeField] private GameObject peepsFather;



    void Start()
    {
        StartCoroutine(CreatePlayers());
    }


    // Update is called once per frame
    void Update()
    {
        if (numPlayersTotal <= numPlayersToWon && !_playerIsDead || gameWon)
        {
            GameFinish(true);
        }
        
        if (_playerIsDead)
        {
            GameFinish(false);
            _playerIsDead = false;
        }
        
        
        if (numPlayersInstantiated == numPlayersTotal && _initiating)
        {
            foreach (var peep in peeps)
            {
                peep.GetComponent<PeepController>().enabled = true;
            }

            _initiating = false;
            asteroidSpawner.gameObject.SetActive(true);
            FindObjectOfType<AudioManager>().PlaySound("backGround");

        }
        
        
    }

    public IEnumerator CreatePlayers()
    {
        bool instantiatePlayer = false;
        float spacePerPlayer = (width - 2) / Mathf.Floor(Mathf.Sqrt(numPlayersTotal));
        float limit = (width - 2) / 2;
        Vector3 pos = new Vector3(-limit, 0, -limit);
        for (float j = -limit + spacePerPlayer / 2; j <= limit; j += spacePerPlayer)
        {
            for (float k = -limit + spacePerPlayer / 2; k <= limit; k += spacePerPlayer)
            {
                var newPos = new Vector3(k, 0, j);
                if (!instantiatePlayer)
                {
                    instantiatePlayer = true;
                    Instantiate(player, newPos, Quaternion.identity);
                    numPlayersInstantiated += 1;
                    //peeps.Add(player);
                    yield return new WaitForSeconds(peepInitializeSpeed);
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, prefabsGroups.Count);
                    while (numPrefabsGroups[randomIndex] == 0)
                    {
                        randomIndex = UnityEngine.Random.Range(0, prefabsGroups.Count);
                    }

                    var newPeep = Instantiate(prefabsGroups[randomIndex], newPos, Quaternion.identity);
                    newPeep.transform.RotateAround(newPeep.transform.position, new Vector3(0,1,0), 180);
                    newPeep.transform.SetParent(peepsFather.transform);
                    numPlayersInstantiated += 1;
                    peeps.Add(newPeep);
                    numPrefabsGroups[randomIndex] -= 1;
                    yield return new WaitForSeconds(peepInitializeSpeed);
                }
            }
        }
    }
    
    
    public void GameFinish(bool mode)
    {
        switch (mode)
        {
            case true:
                gameFinishText.text = "GAME WON";
                break;
            
            case false:
                gameFinishText.text = "GAME OVER";
                break;
        }
        
         gameFinishScreen.gameObject.SetActive(true);
    }
}