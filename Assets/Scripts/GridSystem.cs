using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GridSystem : MonoBehaviour
{
    // land prefab
    public GameObject LandCube;

    // list of all type of materials for land
    public List<Material> LandTypeMaterialArray;

    // list of all land cube
    public List<GameObject> LandArray;


    GameObject PlayerRef;

    GameObject PlayerTurnIndicator;
    GameObject EnemyTurnIndicator;



    void Awake()
    {
        // populate land grid
        for (var x = -4; x < 6; x++)
        {
            for (var z = -4; z < 6; z++)
            {
                // instantiate new land cube
                var newLandCube = Instantiate(LandCube, transform);    
                
                // set position
                newLandCube.transform.position = new Vector3(x - 0.5f, 0, z - 0.5f);

                // set random material
                newLandCube.GetComponent<Renderer>().material = LandTypeMaterialArray[Random.Range(0, LandTypeMaterialArray.Count)];

                // append to land array
                LandArray.Add(newLandCube);


            }
        }

    }

    void Start()
    {
        PlayerTurnIndicator = GameObject.FindGameObjectWithTag("PlayerTurnIndicator");
        EnemyTurnIndicator = GameObject.FindGameObjectWithTag("EnemyTurnIndicator");

        PlayerTurnIndicator.SetActive(true);
        EnemyTurnIndicator.SetActive(false);

        // spawn player
        SpawnPlayer();

        // spawn enemy
        SpawnEnemy();

    }

    

    void SpawnEnemy()
    {
        var ERefList = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var item in ERefList)
        {
            PositionEntityOnGrid(item);
        }
    
    }


    void PositionEntityOnGrid(GameObject entity)
    {
        var RandomLandScript = LandArray[Random.Range(0, LandArray.Count)].GetComponent<LandScript>();

        // set enemy position to random land if no player or enemy on it
        if (RandomLandScript.WhosStandingOnMe == "")
        {
            entity.transform.position = RandomLandScript.transform.position + new Vector3(0,0.5f,0);
            entity.GetComponent<EnemyController>().CurrentLand = RandomLandScript;
        }
        else
        {
            entity.transform.position = RandomLandScript.GetTargetNearByLand(entity.transform.forward).transform.position + new Vector3(0,0.5f,0);
            
        }
    }




    void SpawnPlayer()
    {
        // spawn player
        if (GameObject.FindGameObjectsWithTag("Player").Length <= 0)
        {
            // Instantiate()
        }
        else
        {
            // set player reference
            PlayerRef = GameObject.FindGameObjectWithTag("Player");

            // get random land script
            var RandomLandScript = LandArray[Random.Range(0, LandArray.Count)].GetComponent<LandScript>();

            // set player place to center point of rando land cube
            PlayerRef.transform.position = RandomLandScript.transform.position + new Vector3(0,0.5f,0);
        }
    }





    // goto players turn
    public void PlayerTurn()
    {
        StartCoroutine(CheckIfWin());
        // print("Players Turn..........");

        PlayerTurnIndicator.SetActive(true);
        EnemyTurnIndicator.SetActive(false);

        // active player
        var PController = PlayerRef.GetComponent<PlayerController>();
        PController.PlayerTurn = true;
        PController.AlreadyMoved = false;
        
    }

    // goto enemy's turn
    public void EnemyTurn()
    {
        StartCoroutine(CheckIfWin());
        // print("Enemies Turn..........");

        PlayerTurnIndicator.SetActive(false);
        EnemyTurnIndicator.SetActive(true);

        var AllEnemy = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var item in AllEnemy)
        {
            item.GetComponent<EnemyController>().RoundEvent();
        }

    }




    IEnumerator CheckIfWin()
    {
        yield return new WaitForSeconds(4);

        if (GameObject.FindGameObjectsWithTag("Enemy").Length <= 0)
        {
            SceneManager.LoadScene("WinScene");
        }
    }
}
