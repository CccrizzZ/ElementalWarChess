using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EnemyController : MonoBehaviour
{
    // reference to grid
    GridSystem GridRef;

    // reference to land under enmey
    public LandScript CurrentLand;
    
    
    // if its player's turn
    bool EnemyTurn = true;


    // enemy model ref
    public GameObject EnemyModel;


    // parameters
    public int Health;
    public int BaseDamage;
    public int ElementalBonusDamage;
    public int ElementalReducedDamage;

    // animator
    Animator EnemyAnimator;


    // rotation target
    Quaternion rot;
    bool isRotating = false;

    // destination target
    Vector3 destination;
    bool isWalking = false;

    // player ref
    PlayerController Pref;

    // audio manager
    AudioManager AManager;




    void Start()
    {
        // get audio manager
        AManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        // get player
        Pref = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        // get grid
        GridRef = GameObject.FindGameObjectWithTag("Grid").GetComponent<GridSystem>();
    
        EnemyAnimator = EnemyModel.transform.parent.GetComponent<Animator>();
    

    
    }

    void Update()
    {
        
        // if spawned on a non empty land, goto another one
        if (CurrentLand && CurrentLand.WhosStandingOnMe == "Player")
        {
            transform.position = CurrentLand.GetTargetNearByLand(transform.forward).transform.position;
            
        }

        // return if not in enemy round
        if (!EnemyTurn)return;

        

        // rotate enemy
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

        // move enemy
        if (isWalking && destination != null)
        {
            // lerp to destination
            // transform.position = Vector3.Lerp(transform.position, destination, Time.time * 0.0005f);
            transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * 2f);

            
            // if two positions are nearly equal, stop moving
            if (Vector3.Distance(transform.position, destination) <= 0.1)
            {
                EnemyAnimator.SetBool("IsWalking", false);
                isWalking = false; 

                // player's turn
            }
            
        }
    }




    // update enemy stats when standing on land
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Land"))
        {
            // set current land
            CurrentLand = other.gameObject.GetComponent<LandScript>();

            // set enemy material
            EnemyModel.GetComponent<Renderer>().material = CurrentLand.LandMaterial;

        }
    }


    void EnemyMoveTo(GameObject TargetLand)
    {
        // set look position
        var lookPos = TargetLand.transform.position - transform.position;
        lookPos.y = 0;

        // set rotation angle
        rot = Quaternion.LookRotation(lookPos);

        // start rotating
        isRotating = true;


        var TargetLandScript = TargetLand.GetComponent<LandScript>();
        if (TargetLandScript.WhosStandingOnMe == "Player")
        {
            // Attack Player if player is standing on it
            StartCoroutine(PlayAttackingAnim(TargetLandScript.LandMaterial.name.Replace(" (Instance)", "")));
        }
        else if (IsLandNeighbor(TargetLand) && TargetLandScript.WhosStandingOnMe != "Enemy")
        {
            // if hit land is neighbor of this land and nothing standing on it, move to that

            // set destination
            destination = TargetLand.transform.position + new Vector3(0, 0.5f, 0);

            // set start walking
            isWalking = true;

            // play animation
            EnemyAnimator.SetBool("IsWalking", true);


        }

    }



    // play attack animation
    IEnumerator PlayAttackingAnim(string Target)
    {
        EnemyAnimator.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(1);


        int result = CalcElementalDamage(Target);

        // deduct player health
        if (result == 1)
        {
            
            Pref.TakeDamage(BaseDamage + ElementalBonusDamage);
        }
        else if (result == -1)
        {
            Pref.TakeDamage(BaseDamage - ElementalReducedDamage);
            
        }
        else // 0
        {
            Pref.TakeDamage(BaseDamage);
        }

        EnemyAnimator.SetBool("IsAttacking", false);

    }




    // 0 = base damage
    // 1 = base damage + element damage
    // -1 = base damage - element reduce damage
    int CalcElementalDamage(string TargetType)
    {
        print(TargetType);

        var CurrType = CurrentLand.LandMaterial.name.Replace(" (Instance)", "");
        if (CurrType == "Water")
        {                
            switch (TargetType)
            {
                case "Fire":
                    return 1;
                case "Thunder":
                    return -1;
                default:
                    return 0;
            }
        }
        else if (CurrType == "Fire")
        {
            switch (TargetType)
            {
                case "Grass":
                    return 1;
                case "Water":
                    return -1;
                default:
                    return 0;
            }
        }
        else if (CurrType == "Thunder")
        {
            switch (TargetType)
            {
                case "Water":
                    return 1;
                case "Grass":
                    return -1;
                default:
                    return 0;
            }
        }
        else if (CurrType == "Grass")
        {                
            switch (TargetType)
            {
                case "Fire":
                    return -1;
                case "Thunder":
                    return 1;
                default:
                    return 0;
            }
        }

        return 0;

    }



    // return if target land is neighbor
    bool IsLandNeighbor(GameObject TargetLand)
    {
        return Vector3.Distance(TargetLand.transform.position, CurrentLand.transform.position) <= 10f;
    }


    public void RoundEvent()
    {
        // return if not in enemy round
        if (!EnemyTurn)return;

        var dir = Pref.transform.position - transform.position;
        // print(dir);


        // var TargetLand = CurrentLand.GetTargetNearByLand(transform.forward);
        var TargetLand = CurrentLand.GetTargetNearByLand(dir);

        EnemyMoveTo(TargetLand);


        StartCoroutine(TurnEndIn2Sec());
    }


    IEnumerator TurnEndIn2Sec()
    {
        yield return new WaitForSeconds(2);
        GridRef.PlayerTurn();
    }



    public void TakeDamage(int damage)
    {
        
        AManager.PlaySlash();



        if(Health - damage <= 0)
        {
            Health = 0;
            EnemyAnimator.SetBool("IsDead", true);
            EnemyTurn = false;

            // destroy dead body after couple seconds
            StartCoroutine(DestroyDeadBody());

        }
        else
        {
            Health -= damage;
        }
        print("Enemy Health: " + Health);

    }




    IEnumerator DestroyDeadBody()
    {
        yield return new WaitForSeconds(3);
        
        // clear land tag
        CurrentLand.WhosStandingOnMe = "";

        // destroy body
        Destroy(gameObject);

    }
}
