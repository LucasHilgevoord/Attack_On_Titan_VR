using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject playerHead;

    [SerializeField]
    Transform[] handTargets; //First left, then right

    bool activateGasLeft = false;
    bool activateGasRight = false;
    bool canGrappleJump = true;
    bool useGravityCompensation;
    bool warmUp = false;

    float maxGas = 200; //Volume of tank in seconds of use (using both hands will consume double the amount)
    float currentGas = 200;

    float gasForce = 6;
    float gasTankParameter = 10; //The higher this is, the more "square" the graph looks of force vs tank content
    //Commented below are vars for overheating system
    /*float gasDecayTime = 2;
    float gasTimeToZero = 5; //Basicly maximum overheat
    float currentOverheatLeft = 0;
    float currentOverheatRight = 0;
    float gasCooldownMultiplier = 1;*/

    float jumpForce = 60;
    float maxPullForce = 30;
    float currentPullForce = 0;
    float strongPullForce = 30; //This force will added on top of the normal pullForce, but isn't affected by warming up.
    float pullResistance = 2;
    float stabilizerResistance = 0.1f;
    float breakForce = 50;
    float gravityCompensationForce = 3;
    float gasAssistForce = 3;
    float gasAssistClampFactor = 0.1f;

    float warmpUpSpeed = 50;
    float groundResistance = 3f;
    float walkingForce = 20;
    float maxWalkSpeed = 5;
    float rotationMultiplier = 100;

    float[] pullCounter = { 0, 0 }; //For being able to pull grapples so they pull back harder
    float pullCounterTotalThreshold = 3f;
    float pullCounterAccThreshold = 5; //Minimum acceleration of hands to activate the strong pull

    Vector3 walkingVector3;

    Vector3 grappleVec;

    [SerializeField]
    HookBehavior[] grapples; //First left, then right

    [SerializeField]
    GameObject smokeParticles;

    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject rightHand;
    GameObject[] hands;

    Vector3[] handPositions;
    Vector3[] prevHandPositions;
    Vector3[] beforePrevHandPositions;
    Vector3[] relativeDisplacement;
    Vector3[] prevRelativeDisplacement;
    float speedDifferenceProjection = 0;
    bool[] strongPull;

    Rigidbody rb;
    Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        //JumpForce();
        prevHandPositions = new Vector3[] { Vector3.zero, Vector3.zero };
        beforePrevHandPositions = new Vector3[] { Vector3.zero, Vector3.zero };
        relativeDisplacement = new Vector3[] { Vector3.zero, Vector3.zero };
        prevRelativeDisplacement = new Vector3[] { Vector3.zero, Vector3.zero };

        hands = new GameObject[] { leftHand, rightHand };

        strongPull = new bool[] { false, false };
    }

    // Update is called once per frame
    void Update()
    {
        //Teleport player back when falling from map beyond saving
        if(transform.position.y < -100)
        {
            transform.position = Vector3.zero;
            rb.velocity = Vector3.zero;
        }


        if (SteamVR_Actions.player_controller.grip_left.state)
        {
            activateGasLeft = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasLeft = false;
        }

        if (SteamVR_Actions.player_controller.grip_right.state)
        {
            activateGasRight = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasRight = false;
        }

        //Relative displacement of hands
        for (int i = 0; i < grapples.Length; i++)
        {
            handPositions = new Vector3[] { leftHand.transform.localPosition, rightHand.transform.localPosition };
            relativeDisplacement[i] = handPositions[i] - prevHandPositions[i];
            prevRelativeDisplacement[i] = prevHandPositions[i] - beforePrevHandPositions[i];

            beforePrevHandPositions[i] = prevHandPositions[i];
            prevHandPositions[i] = handPositions[i];

            if (grapples[i].collided)
            {
                //Calculate difference in speed and compute the projection of that onto grappleVec. This will be related to acceleration to/from the grapple
                speedDifferenceProjection = Vector3.Dot((relativeDisplacement[i] - prevRelativeDisplacement[i]) / Time.deltaTime, grappleVec);
                if (speedDifferenceProjection < -pullCounterAccThreshold * Time.deltaTime) //Note: difference in speed = difference in velocity times delta time
                {
                    pullCounter[i] += -speedDifferenceProjection;
                }
                else
                {
                    pullCounter[i] = 0;
                }

                if (pullCounter[i] > pullCounterTotalThreshold)
                {
                    strongPull[i] = true;
                }
            }
        }

    }

    void FixedUpdate()
    {
        if (activateGasLeft) useGas(0); //Gas left side
        if (activateGasRight) useGas(1); //Gas right side

        if(IsGrounded()) walkAndSlideForce();
        GrapplePullForce();

        //player rotation
        if (SteamVR_Actions.player_controller.TP_right_press.state)
        {
            Vector2 rotationVector = SteamVR_Actions.player_controller.TP_right_vector.axis;
            transform.Rotate(0, rotationVector.x * rotationMultiplier * Time.deltaTime, 0);
        }

    }

    void GrapplePullForce()
    {

        useGravityCompensation = false;
        warmUp = false;
        for (int i = 0; i < grapples.Length; i++)
        {
            if (grapples[i].collided)
            {
                //grapple warming up
                warmUp = true;

                if (currentPullForce > maxPullForce) { currentPullForce = maxPullForce; }

                grappleVec = (grapples[i].transform.position - transform.position).normalized;

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
                    //if(!activateGas) rb.AddForce(rb.velocity * stabilizerResistance * -1);
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

                //Strong pull
                if (strongPull[i])
                {
                    rb.AddForce(grappleVec * strongPullForce);
                }

            }
            else
            {
                strongPull[i] = false;
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

    void JumpForce()
    {
        //when on the ground and gas button is activated, jump up and not forward anymore
        rb.AddForce(new Vector3(0, 1, 0) * jumpForce);
    }

    bool IsGrounded()
    {
        float DistanceToTheGround = col.bounds.extents.y;
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, DistanceToTheGround + 0.1f);
    }

    void useGas(int side)
    {
        //Code to weaken force when tank is almost empty
        float tankRatio = currentGas / maxGas;
        float amplitude = 1 / (1 - Mathf.Exp(-gasTankParameter));
        float strength = gasForce * amplitude * (1 - Mathf.Exp(-gasTankParameter * tankRatio));
        rb.AddForce(handTargets[side].right * Mathf.Pow(-1, side) * strength); //Note: side=0 is left, side=1 is right
        //Also note: For the right hand (side=1) I need the right vector flipped, that's where the power of -1 is for.
        currentGas -= Time.deltaTime;
        if (currentGas < 0) currentGas = 0;
    }

    void walkAndSlideForce()
    {
        Vector2 trackPadVector = SteamVR_Actions.player_controller.TP_left_vector.axis;
        Vector2 playerForward = new Vector2(playerHead.transform.forward.x, playerHead.transform.forward.z);
        Vector2 playerRight = new Vector2(playerHead.transform.right.x, playerHead.transform.right.z);
        Vector2 walkingVector = trackPadVector.y * playerForward + trackPadVector.x * playerRight;
        walkingVector3 = new Vector3(walkingVector.x, 0, walkingVector.y);

        if (SteamVR_Actions.player_controller.TP_left_press.state)
        {

            if (rb.velocity.magnitude < maxWalkSpeed * walkingVector3.magnitude)
            {
                rb.AddForce(walkingVector3 * walkingForce);
            }
            else
            {
                rb.AddForce(rb.velocity.normalized * groundResistance * -1);
            }

        }
        else
        {
            rb.AddForce(rb.velocity.normalized * groundResistance * -1);
        }
    }
}