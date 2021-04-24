using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandScript : MonoBehaviour
{
    
    public Material LandMaterial;

    public Vector3 CenterPoint;


    public string WhosStandingOnMe;


    void OnEnable()
    {
        // get center point
        CenterPoint = transform.position - new Vector3(0.5f, 0.0f, 0.5f);
        
    }

    void Start()
    {

        // get material
        LandMaterial = GetComponent<Renderer>().material;
        
        
    }

    void Update()
    {

    }


    void OnCollisionEnter(Collision other)
    {
        // print(other.gameObject.name);
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        WhosStandingOnMe = other.gameObject.tag;
        // print(WhosStandingOnMe);

    }


    void OnCollisionExit(Collision other) 
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy")) return;

        WhosStandingOnMe = "";
        // print("TagReset");
    }



    public GameObject GetTargetNearByLand(Vector3 forward)
    {
        var ray = new Ray(transform.position + new Vector3(0, 0.1f, 0), forward);
        RaycastHit hit;

        // get the tile near it
        if (Physics.Raycast(ray, out hit, 1.1f))
        {
            // print(hit.transform.gameObject.name);
            
            if (hit.transform.gameObject.CompareTag("Land"))
            {
                // print(hit.transform.gameObject.GetComponent<LandScript>().LandMaterial);

                return hit.transform.gameObject;

            }
        }
        return null;

    }
}
