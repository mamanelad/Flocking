using System.Collections;
using System.Collections.Generic;
using Flocking;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    #region Private Fields

    private readonly List<GameObject> peeps = new List<GameObject>();
    private int numPlayersInstantiated;
    private bool initiating = true;

    #endregion

    #region Inspector Control

    [SerializeField] private bool gameWon;
    [SerializeField] private bool playerIsDead;

    [SerializeField] public int numPlayersTotal; // including the player
    [SerializeField] private float numPlayersToWon = 10f;
    [SerializeField] private float peepInitializeSpeed = 0.05f;
    [SerializeField] private float width; // width (=height) of the stage

    [SerializeField] private GameObject gameFinishScreen;
    [SerializeField] private Text gameFinishText;

    [Header("Needed Game Objects")] [SerializeField]
    private AsteroidSpawner asteroidSpawner;

    [SerializeField] private GameObject player; // for now will be red leader

    [SerializeField] private List<GameObject> prefabsGroups; // the prefabs of followers, one from each

    // group - no player
    [SerializeField] private List<int> numPrefabsGroups; // how much followers we want from each group
    [SerializeField] private GameObject peepsFather;

    #endregion

    private void Start()
    {
        StartCoroutine(CreatePlayers());
    }
    
    private void Update()
    {
        if (numPlayersTotal <= numPlayersToWon && !playerIsDead || gameWon)
        {
            GameFinish(true);
        }

        if (playerIsDead)
        {
            GameFinish(false);
            playerIsDead = false;
        }


        if (numPlayersInstantiated == numPlayersTotal && initiating)
        {
            foreach (var peep in peeps)
            {
                peep.GetComponent<PeepController>().enabled = true;
            }

            initiating = false;
            asteroidSpawner.gameObject.SetActive(true);
            FindObjectOfType<AudioManager>().PlaySound("backGround");
        }
    }
    
    private IEnumerator CreatePlayers()
    {
        var instantiatePlayer = false;
        var spacePerPlayer = (width - 2) / Mathf.Floor(Mathf.Sqrt(numPlayersTotal));
        var limit = (width - 2) / 2;
        for (var j = -limit + spacePerPlayer / 2; j <= limit; j += spacePerPlayer)
        {
            for (var k = -limit + spacePerPlayer / 2; k <= limit; k += spacePerPlayer)
            {
                var newPos = new Vector3(k, 0.2f, j);
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
                    var randomIndex = Random.Range(0, prefabsGroups.Count);
                    while (numPrefabsGroups[randomIndex] == 0)
                    {
                        randomIndex = Random.Range(0, prefabsGroups.Count);
                    }

                    var newPeep = Instantiate(prefabsGroups[randomIndex], newPos, Quaternion.identity);
                    newPeep.transform.RotateAround(newPeep.transform.position, new Vector3(0, 1, 0), 180);
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
        asteroidSpawner.spawn = false;
        gameFinishText.text = mode switch
        {
            true => "GAME WON",
            false => "GAME OVER"
        };

        gameFinishScreen.gameObject.SetActive(true);
    }
}