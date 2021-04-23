using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{


    // reference to land under enmey
    LandScript CurrentLand;


    void Start()
    {

    }

    void Update()
    {
        
    }




    // update enemy stats when standing on land
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Land"))
        {
            // set current land
            CurrentLand = other.gameObject.GetComponent<LandScript>();

            // set enemy material
            GetComponent<Renderer>().material = CurrentLand.LandMaterial;

            // // set whos standing on land
            // CurrentLand.WhosStandingOnMe = tag;
            // print(CurrentLand.WhosStandingOnMe);
        }
    }


}
