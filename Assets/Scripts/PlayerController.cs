using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    
    Rigidbody2D rigidBody;                  //Variable storing the rigidbody component of the player asset
    Vector2 boxExtents;                     //Used for ground detection
    Animator animator;

    //-Player physics-
    float horizMaxSpeed = 15.0f;            //Ground movement speed cap (doewsn't prevent player being launched at higher speeds)
    float groundControlSpeed = 1.0f;        //Dictates how fast force is applied to the player when they are moving on the ground
    float airControlSpeed = 30.0f;          //Force responcible for amount of control allowed
    float airControlMaxForce = 10.0f;       //Maximum force allowed in air, when the player is attempting to move mid-air
    float sprintSpeedMultiplier = 0.5f;     //Multiplier for how much faster the player can move while sprinting
    float terminalVelocity = 100.0f;

    float jumpForce = 10.0f;                //Vertical force component = how much force to add upon a player's jump action.
    private float fallingSpeed = 1.0f;      //How fast the player falls when holding down when mid-air

    private int extraJumps = 0;              //How many more jumps the player can do in the air
    private int extraJumpCount = 0;         //Keeps track of how many extra jumps the player has done

    private bool dead = false;
    private string finalLevel = "4";

    

    //-Scene information-
    //Stores name of current & next level (if there is no next level, "END" is stored instead)
    string curLevel;
    string nextLevel;

    //-UI Elements-
    public TextMeshProUGUI uiDeathText;
    public TextMeshProUGUI uiLevelInfo;

    //-Audio Sources-
    public AudioSource jumpSound;
    public AudioSource deathSound;
    public AudioSource nextLevelSound;
    public AudioSource crumbleSound;
    public AudioSource WinSound;

    IEnumerator DoDeath()
    {
        dead = true;
        animator.SetBool("dead", true);
        deathSound.Play();
        uiDeathText.enabled = true;

        yield return new WaitForSeconds(0.5f);// reload the level in 2 seconds
        
    }

    IEnumerator LoadNextLevel()
    {
        if (nextLevel != "END")
        {
            // hides & freezes the player, waits a bit and load new scene.
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            GetComponent<Renderer>().enabled = false;

            nextLevelSound.Play();
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene(nextLevel);
        }
        else
        {
            rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            GetComponent<Renderer>().enabled = false;

            yield return null;
            uiDeathText.color = Color.yellow;
            uiDeathText.text = "YOU WIN!";
            uiDeathText.enabled = true;
            WinSound.Play();
        }
    }

    IEnumerator CrumbleObject( Collision2D collision)
    {

        yield return new WaitForSeconds(0.5f);
        crumbleSound.Play();
        Destroy(collision.gameObject);

    }

    // Start is called before the first frame update
    void Start()
    {
        uiDeathText.enabled = false;

        rigidBody = GetComponent<Rigidbody2D>();
        boxExtents = GetComponent<CircleCollider2D>().bounds.extents;// get the extent of the collision box
        animator = GetComponent<Animator>();


        //-Scene information-
        curLevel = SceneManager.GetActiveScene().name;
        uiLevelInfo.text = curLevel;
        nextLevel = "Level ";
        if (curLevel.Substring(curLevel.Length - 1) == finalLevel)
        {
            nextLevel = "END";
        }
        else
        {
            nextLevel += (int.Parse(curLevel.Substring(curLevel.Length - 1)) + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transforms the player object to have a negative scale if the player has a negative x velocity
        if (rigidBody.velocity.x * transform.localScale.x < 0.0f)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        if (Input.GetAxis("Cancel") != 0.0f)
        {
            SceneManager.LoadScene("Menu");
        }
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = Vector2.ClampMagnitude(rigidBody.velocity, terminalVelocity);

        if (!dead)
        {
            float horizInput = Input.GetAxis("Horizontal"); // -1, 0, or 1 depending on user input
            float vertInput = Input.GetAxis("Vertical"); // -1, 0, or 1 depending on user input

            // check if we are on the ground
            Vector2 bottom = new Vector2(transform.position.x, transform.position.y - boxExtents.y);
            Vector2 hitBoxSize = new Vector2(boxExtents.x * 2.0f, 0.05f);
            RaycastHit2D result = Physics2D.BoxCast(bottom, hitBoxSize, 0.0f, new Vector3(0.0f, -1.0f), 0.0f, 1 << LayerMask.NameToLayer("Ground"));
            bool grounded = result.collider != null && result.normal.y > 0.9f;


            if (grounded)
            {
                if (Input.GetAxis("Jump") > 0.0f) //UPON PLAYER JUMP INPUT
                {
                    rigidBody.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
                    extraJumpCount = 0;
                    jumpSound.Play();
                    animator.SetBool("jumping",true);

                }
                else
                {
                    //movement of the player is different depending on if they are holding down left shift or not
                    //if statement checks if the player is already moving at terminal ground velocity (which is increased for when they are holding shift)
                    //if player isn't moving at terminal ground velocity, a force is added in the desired direction & with the sprint modifier if left shift is held down
                    if ((Math.Abs(rigidBody.velocity.x + horizInput * groundControlSpeed) < (horizMaxSpeed + horizMaxSpeed * Input.GetAxis("Fire3") * sprintSpeedMultiplier)) && horizInput != 0)
                        rigidBody.AddForce(new Vector2(horizInput * (groundControlSpeed + (Input.GetAxis("Fire3") * sprintSpeedMultiplier * groundControlSpeed)), 0.0f), ForceMode2D.Impulse);
                    animator.SetBool("jumping", false);
                }
            }
            else//If player character is in the air
            {
                //allows for multiple jumps
                //checks if the verical velocity is below a threshhold, as to prevent the jumps from being used up accidentally when the button is held for too long
                if (Input.GetAxis("Jump") > 0.0f && extraJumps != extraJumpCount && rigidBody.velocity.y <= 3.0f)
                {
                    rigidBody.velocity = (new Vector2(rigidBody.velocity.x, 0.0f));
                    rigidBody.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
                    extraJumpCount += 1;

                }
                // Air control / strafing
                float vx = rigidBody.velocity.x;
                if (horizInput * vx < airControlMaxForce)
                    rigidBody.AddForce(new Vector2(horizInput * airControlSpeed, 0));

                //fast fall mechanic when player holds down movement key
                if (vertInput == -1)
                {
                    rigidBody.AddForce(new Vector2(0, vertInput * fallingSpeed), ForceMode2D.Impulse);
                }
            }
        }
        else
        {
            if(Input.GetAxis("Fire1") > 0.0f && uiDeathText.text != "YOU WIN!")
            {
                SceneManager.LoadScene(curLevel);
                
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Win")
        {
            StartCoroutine(LoadNextLevel());
        }
        else if (collision.gameObject.tag == "Kill")
        {
            StartCoroutine(DoDeath());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Explosive")
        {
            StartCoroutine(DoDeath());
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Crumbling")
        {
            StartCoroutine(CrumbleObject(collision));
        }
        
    }
    
}
