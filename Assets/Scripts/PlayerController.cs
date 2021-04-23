using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // reference to land under player
    LandScript CurrentLand;

    // if its player's turn
    bool PlayerTurn = true;

    // player model ref
    public GameObject PlayerModel;


    // animator
    public Animator PlayerAnimator;
    
    // rotation target
    Quaternion rot;
    bool isRotating = false;

    // destination target
    Vector3 destination;
    bool isWalking = false;

    void Start() 
    {
        
    }


    void Update()
    {
        // return if not in player round
        if (!PlayerTurn)return;

        // mouse button event
        if (Input.GetMouseButtonDown(0))
        {

            // raycast from camera
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // get hit object
                var HitGameObject = hit.transform.gameObject;

                if (HitGameObject.CompareTag("Player"))return;


                // return if clicked on current land or not land or is not enemy
                if (
                    HitGameObject == CurrentLand.gameObject 
                    && !HitGameObject.CompareTag("Land") 
                    && !HitGameObject.CompareTag("Enemy")
                ) return;
               
                // set look position
                var lookPos = HitGameObject.transform.position - transform.position;
                lookPos.y = 0;

                // set rotation angle
                rot = Quaternion.LookRotation(lookPos);

                // start rotating
                isRotating = true;


                
                // if clicked enemy
                if (HitGameObject.CompareTag("Enemy"))
                {

                    print("Attacked");
                    StartCoroutine(PlayAttackingAnim());
                }
                else if (HitGameObject.CompareTag("Land"))
                {
                    // if clicked on land
                    // get land script
                    var HitLandScript = HitGameObject.GetComponent<LandScript>();
                    
                    // if hit land is neighbor of this land and nothing standing on it
                    if (IsLandNeighbor(HitGameObject) && HitLandScript.WhosStandingOnMe == "")
                    {
                        // move to that land
                        // MoveToLand(HitGameObject);


                        // set destination
                        destination = HitGameObject.transform.position + new Vector3(0, 0.5f, 0);

                        // set start walking
                        isWalking = true;

                        // play animation
                        PlayerAnimator.SetBool("IsWalking", true);


                    }
                    
                }

                
            }
        }
    

        // rotate player
        if (isRotating && rot != null)
        {
            // lerp to target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10);
            
            // if two rotations are nearly equal, stop rotating
            if (Mathf.Abs(Quaternion.Dot(transform.rotation, rot)) >= 0.999f)
            {
                isRotating = false;

            }
        }

        // move player
        if (isWalking && destination != null)
        {
            // lerp to destination
            // transform.position = Vector3.Lerp(transform.position, destination, Time.time * 0.0005f);
            transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * 2f);

            
            // if two positions are nearly equal, stop moving
            if (Vector3.Distance(transform.position, destination) <= 0.1)
            {
                isWalking = false; 
                PlayerAnimator.SetBool("IsWalking", false);
            }
            // print(isWalking);
        }

    }


    IEnumerator PlayAttackingAnim()
    {
        PlayerAnimator.SetBool("IsAttacking", true);
        // print(PlayerAnimator.)
        yield return new WaitForSeconds(1);
        PlayerAnimator.SetBool("IsAttacking", false);

    }


    void MoveToLand(GameObject TargetLand)
    {
        transform.position = TargetLand.transform.position + new Vector3(0, 0.5f, 0);
    }

    bool IsLandNeighbor(GameObject TargetLand)
    {
        return Vector3.Distance(TargetLand.transform.position, CurrentLand.transform.position) <= 10f;
    }

    // update player stats when standing on land
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Land"))
        {
            // set current land player is on
            CurrentLand = other.gameObject.GetComponent<LandScript>();

            // set player material
            PlayerModel.GetComponent<Renderer>().material = CurrentLand.LandMaterial;

            // set object above land for current land
            // CurrentLand.WhosStandingOnMe = tag;
            // print(CurrentLand.WhosStandingOnMe);
        }
    }


}
