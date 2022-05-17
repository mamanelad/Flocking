using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avrahamy.Collections;
using Flocking;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject player; // for now will be red leader
    [SerializeField] private GameObject bluePlayer;
    

    [SerializeField] private float width;
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
                    peeps.Add(player);
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    var newPeep = Instantiate(bluePlayer, newPos, Quaternion.identity);
                    numPlayersInstantiated += 1;
                    peeps.Add(newPeep);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }


    }
}
