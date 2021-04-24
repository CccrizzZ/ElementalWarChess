using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    AudioManager AManager;


    // reference to grid
    GridSystem GridRef;

    // reference to land under player
    LandScript CurrentLand;

    // if its player's turn
    public bool PlayerTurn = true;

    // if player already moved this round
    public bool AlreadyMoved;

    // deal bool
    bool isDead;


    // player model ref
    public GameObject PlayerModel;

    // player health
    public int Health = 100;

    // animator
    public Animator PlayerAnimator;
    
    // rotation target
    Quaternion rot;
    bool isRotating = false;

    // destination target
    Vector3 destination;
    bool isWalking = false;

    // healthbar ref
    GameObject HealthBar;

    // damage
    public int BaseDamage = 40;
    public int ElementalBonusDamage = 60;
    public int ElementalReducedDamage = 20;


    // pause menu
    public GameObject PauseUI;

    public bool isPaused;
    


    void Start()
    {
    
        isPaused = false;
        isDead = false;

        // Get health bar
        HealthBar = GameObject.FindGameObjectWithTag("HealthBar");

        // get audio manager
        AManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
       
        // get grid
        GridRef = GameObject.FindGameObjectWithTag("Grid").GetComponent<GridSystem>();
        AlreadyMoved = false;
    }


    void Update()
    {
        // return if paused
        if (isPaused)return;

        // return if dead
        if (isDead)return;

        // return if not in player round
        if (!PlayerTurn)return;


        // rotate player
        if (isRotating && rot != null)
        {
            // lerp to target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10);
            
            // if two rotations are nearly equal, stop rotating
            if (Mathf.Abs(Quaternion.Dot(transform.rotation, rot)) >= 0.999f)
            {
                isRotating = false;

                // print(transform.right);
                CurrentLand.GetTargetNearByLand(transform.right);
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

                // Enemy's turn
                SetEnemyTurn();

            }
        }

    }


    // pause
    void OnPause(InputValue input)
    {
        if (PauseUI.activeInHierarchy == false)
        {
            PauseUI.SetActive(true);
            // set time scale
            Time.timeScale = 0;
            isPaused = true;

        }
        else
        {
            PauseUI.SetActive(false);
            // set time scale
            Time.timeScale = 1;
            isPaused = false;
        }

    }


    // mouse button event
    void OnFire(InputValue mouse)
    {
        // return if paused
        if (isPaused)return;


        // return if dead
        if (isDead)return;

        // print("AlreadyMoved" + AlreadyMoved);


        // return if already moved for this round
        if (AlreadyMoved)return;

        // raycast from camera
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
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
            




            
            // if clicked enemy
            if (HitGameObject.CompareTag("Enemy"))
            {
                if (Vector3.Distance(HitGameObject.transform.position, transform.position) > 1.5f) return;
                StartCoroutine(PlayAttackingAnim(HitGameObject));

                // set rotation and already clicked to true
                SetRotation(HitGameObject);
            }
            else if (HitGameObject.CompareTag("Land"))
            {
                // if clicked on land
                // get land script
                var HitLandScript = HitGameObject.GetComponent<LandScript>();
                
                // if hit land is neighbor of this land and nothing standing on it
                if (IsLandNeighbor(HitGameObject) && HitLandScript.WhosStandingOnMe == "")
                {

                    // set destination
                    destination = HitGameObject.transform.position + new Vector3(0, 0.5f, 0);

                    // set start walking
                    isWalking = true;

                    // play animation
                    PlayerAnimator.SetBool("IsWalking", true);
                    
                    // set rotation and already clicked to true
                    SetRotation(HitGameObject);

                }
            }

            
        }
    }

    // play attack animation
    IEnumerator PlayAttackingAnim(GameObject TargetEnemy)
    {
        PlayerAnimator.SetBool("IsAttacking", true);


        yield return new WaitForSeconds(0.2f);

        // deduct enemy health
        // TargetEnemy.GetComponent<EnemyController>().TakeDamage(20);
        var EController = TargetEnemy.GetComponent<EnemyController>();

        // see if element effect
        var typename = EController.CurrentLand.LandMaterial.name.Replace(" (Instance)", "");
        int result = CalcElementalDamage(typename);

        // deduct enemy health
        if (result == 1)
        {
            
            EController.TakeDamage(BaseDamage + ElementalBonusDamage);


            print("More Damage");
        }
        else if (result == -1)
        {
            EController.TakeDamage(BaseDamage - ElementalReducedDamage);
            print("Less Damage");
            
        }
        else // 0
        {
            print("No effect");

            EController.TakeDamage(BaseDamage);
        }



        yield return new WaitForSeconds(1);



        PlayerAnimator.SetBool("IsAttacking", false);


        // Enemy round
        SetEnemyTurn();


    }



    // 0 = base damage
    // 1 = base damage + element damage
    // -1 = base damage - element reduce damage
    int CalcElementalDamage(string TargetType)
    {

        var CurrType = CurrentLand.LandMaterial.name.Replace(" (Instance)", "");
        print(CurrType + " -> Attaking -> " + TargetType);
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


    void SetRotation(GameObject Target)
    {
        // set look position
        var lookPos = Target.transform.position - transform.position;
        lookPos.y = 0;

        // set rotation angle
        rot = Quaternion.LookRotation(lookPos);

        // start rotating

        isRotating = true;
        AlreadyMoved = true;
    }


    void SetEnemyTurn()
    {
        PlayerTurn = false;
        GridRef.EnemyTurn();
    }


    bool IsLandNeighbor(GameObject TargetLand)
    {
        return Vector3.Distance(TargetLand.transform.position, CurrentLand.transform.position) <= 1f;
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

        }
    }


    // player take damage
    public void TakeDamage(int damage)
    {
        AManager.PlaySlash();


        if(Health - damage <= 0)
        {
            Health = 0;
            PlayerAnimator.SetBool("IsDead", true);
            isDead = true;

            // player death
            StartCoroutine(DeathScene());


        }
        else
        {
            Health -= damage;
        }

        print("Player Health: " + Health);

        // update UI
        HealthBar.transform.localScale = new Vector3(Health / 100f, 1, 1);

        if (Health <= 60 && Health > 30)
        {
            HealthBar.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
        }
        else if(Health <= 30)
        {
            HealthBar.transform.GetChild(0).GetComponent<Image>().color = Color.red;
        }
        else
        {
            HealthBar.transform.GetChild(0).GetComponent<Image>().color = Color.green;
        }

    }


    IEnumerator DeathScene()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("LostScene");
    }

}
