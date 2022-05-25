using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avrahamy.Collections;
using Flocking;
using UnityEngine;


public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject player; // for now will be red leader
    [SerializeField] private List<GameObject> prefabsGroups; // the prefabs of followers, one from each group - no player
    [SerializeField] private List<int> numPrefabsGroups; // how much followers we want from each group
    [SerializeField] private float width; // width (=height) of the stage
    [SerializeField] private int numPlayersTotal; // including the player
    
    private List<GameObject> peeps = new List<GameObject>();
    private int numPlayersInstantiated = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreatePlayers());
    }

    
    // Update is called once per frame
    void Update()
    {
        if (numPlayersInstantiated == numPlayersTotal)
        {
            foreach (var peep in peeps)
            {
                peep.GetComponent<PeepController>().enabled = true;
            }
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
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, prefabsGroups.Count);
                    while (numPrefabsGroups[randomIndex] == 0)
                    {
                        randomIndex = UnityEngine.Random.Range(0, prefabsGroups.Count);
                    }
                    var newPeep = Instantiate(prefabsGroups[randomIndex], newPos, Quaternion.identity);
                    numPlayersInstantiated += 1;
                    peeps.Add(newPeep);
                    numPrefabsGroups[randomIndex] -= 1;
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }


    }
}
