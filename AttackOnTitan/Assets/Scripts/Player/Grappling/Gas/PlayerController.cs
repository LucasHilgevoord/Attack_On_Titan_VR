using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject gasPoint;
    [SerializeField]
    GameObject playerHead;

    bool activateGas = false;
    bool activatePull = false;
    bool onGround = false;
    bool canGrappleJump = true;
    bool useGravityCompensation;
    bool warmUp = false;

    float maxGas = 100;
    float currentGass = 100;

    float gasForce = 10;
    float jumpForce = 60;
    float maxPullForce = 50;
    float currentPullForce = 0;
    float pullResistance = 2;
    float stabilizerResistance = 0.1f;
    float breakForce = 50;
    float gravityCompensationForce = 3;
    float gasAssistForce = 5;
    float gasAssistClampFactor = 0.1f;

    float warmpUpSpeed = 50;
    float groundResistance = 3f;
    float walkingForce = 20;
    float maxWalkSpeed = 5;
    float rotationMultiplier = 100;

    Vector3 walkingVector3;

    GameObject[] grapplePoints;
    [SerializeField]
    GameObject smokeParticles;

    Rigidbody rb;
    Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        //JumpForce();
    }

    // Update is called once per frame
    void Update()
    {
        if (SteamVR_Actions.player_controller.grip.state)
        {
            activateGas = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGas = false;
        }

        GrapplePullForce();

    }

    void FixedUpdate()
    {
        //direction of L-pad, making relative to playerHead.transform.forward
        if(SteamVR_Actions.player_controller.TP_left_press.state)
        {
            Vector2 trackPadVector = SteamVR_Actions.player_controller.TP_left_vector.axis;
            Vector2 playerForward = new Vector2(playerHead.transform.forward.x, playerHead.transform.forward.z);
            Vector2 playerRight = new Vector2(playerHead.transform.right.x, playerHead.transform.right.z);
            Vector2 walkingVector = trackPadVector.y * playerForward + trackPadVector.x * playerRight;
            walkingVector3 = new Vector3(walkingVector.x, 0, walkingVector.y);
        }

        if (activateGas && !IsGrounded()) HeadRotForce();
        if (activateGas && IsGrounded()) JumpForce();
        if (IsGrounded())
        {
            //walking
            if(SteamVR_Actions.player_controller.TP_left_press.state)
            {
                
                if (rb.velocity.magnitude < maxWalkSpeed * walkingVector3.magnitude)
                {
                    rb.AddForce(walkingVector3 * walkingForce);
                }
                else
                {
                    rb.AddForce(rb.velocity.normalized * groundResistance * -1);
                }

            }else
            {
                rb.AddForce(rb.velocity.normalized * groundResistance * -1);
            }
        }
        if (activatePull) GrapplePullForce();

        //player rotation
        if (SteamVR_Actions.player_controller.TP_right_press.state)
        {
            Vector2 rotationVector = SteamVR_Actions.player_controller.TP_right_vector.axis;
            transform.Rotate(0, rotationVector.x * rotationMultiplier * Time.deltaTime, 0);
        }

    }

    void GrapplePullForce()
    {

        grapplePoints = GameObject.FindGameObjectsWithTag("Grapple");
        if (grapplePoints.Length > 0)
        {
            useGravityCompensation = false;
            warmUp = false;
            for (int i = 0; i < grapplePoints.Length; i++)
            {
                if (grapplePoints[i].GetComponent<HookBehavior>().collided)
                {
                    //grapple warming up
                    warmUp = true;
                    
                    if (currentPullForce > maxPullForce) { currentPullForce = maxPullForce; }

                    Vector3 grappleVec = (grapplePoints[i].transform.position - transform.position).normalized;

                    //Resistance
                    float angle = Vector3.Angle(grappleVec, rb.velocity);
                    float velocityToGrapple = rb.velocity.magnitude * Mathf.Cos((angle * Mathf.PI) / 180);
                    //Forces
                    if (angle > 90)
                    {
                        //Player infront of average point
                        rb.AddForce(grappleVec * currentPullForce);
                        rb.AddForce(grappleVec * breakForce * velocityToGrapple * -1);
                        //Use stabilizer if not using gas
                        if(!activateGas) rb.AddForce(rb.velocity * stabilizerResistance * -1);
                    }
                    else
                    {
                        rb.AddForce(grappleVec * currentPullForce);
                        rb.AddForce(grappleVec * pullResistance * velocityToGrapple * -1);
                    }

                    //Gas Assist (makes player move more straight in the direction of the grapple and less around it)
                    Vector3 velVec = rb.velocity.normalized;
                    Vector3 gasAssistVec = -1 * velVec + Vector3.Dot(grappleVec, velVec) * grappleVec;
                    rb.AddForce(gasAssistVec * gasAssistForce * Mathf.Clamp01(rb.velocity.magnitude * gasAssistClampFactor));


                    //begin jump
                    if (IsGrounded() && canGrappleJump)
                    {
                        JumpForce();
                        canGrappleJump = false;
                    }
                    if (!IsGrounded() && canGrappleJump) canGrappleJump = false;

                    //for gravity compensation
                    useGravityCompensation = true;

                }
            }

            if (useGravityCompensation)
            {
                //anti-gravity compensation with gas
                rb.AddForce(new Vector3(0, 1, 0) * gravityCompensationForce);
            }

            if (warmUp)
            {
                currentPullForce += warmpUpSpeed * Time.deltaTime;
            }
            else
            {
                //grapple cooling down
                currentPullForce -= warmpUpSpeed * Time.deltaTime;
                if (currentPullForce < 0) { currentPullForce = 0; }
            }
        }
        else
        {
            canGrappleJump = true;
        }
    }

    void HeadRotForce()
    {
        //When in the air and gas button is pressed, go to the direction you are facing
        if(SteamVR_Actions.player_controller.TP_left_press.state)
        {
            rb.AddForce(walkingVector3 * gasForce);
        }
        else
        {
            //rb.AddForce(new Vector3(playerHead.transform.forward.x, 0, playerHead.transform.forward.z) * gasForce);
            //rb.AddForce(rb.velocity.normalized * gasForce);
            rb.AddForce(playerHead.transform.forward * gasForce);
        }
    }

    void JumpForce()
    {
        //when on the ground and gas button is activated, jump up and not forward anymore
        rb.AddForce(new Vector3(0, 1, 0) * jumpForce);
    }

    bool IsGrounded()
    {
        float DisstanceToTheGround = col.bounds.extents.y;
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, DisstanceToTheGround + 0.1f);
    }
}