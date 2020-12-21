using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    #region parameters
    [Header("Components")]
    [SerializeField] private GameObject playerHead;
    private Rigidbody rb;
    private Collider col;

    [Header("Stats")]
    private float groundResistance = 3;
    private float walkingForce = 20;
    private float maxWalkSpeed = 5;

    [Header("Options")]
    private float rotationMultiplier = 100;
    
    [Header("Calculation vars")]
    internal Vector3 walkingVector3;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        //Teleport player back when falling from map beyond saving
        if (transform.position.y < -100)
        {
            transform.position = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if(IsGrounded()) walkAndSlideForce();

        //player rotation
        if (SteamVR_Actions.player_controller.TP_right_press.state)
        {
            Vector2 rotationVector = SteamVR_Actions.player_controller.TP_right_vector.axis;
            transform.Rotate(0, rotationVector.x * rotationMultiplier * Time.deltaTime, 0);
        }
    }

    internal bool IsGrounded()
    {
        float DistanceToTheGround = col.bounds.extents.y;
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, DistanceToTheGround + 0.1f);
    }

    private void walkAndSlideForce()
    {
        updateWalkingVector();

        if (SteamVR_Actions.player_controller.TP_left_press.state)
        {

            if (rb.velocity.magnitude < maxWalkSpeed * walkingVector3.magnitude)
                rb.AddForce(walkingVector3 * walkingForce);
            else
                rb.AddForce(rb.velocity.normalized * groundResistance * -1);

        }
        else
            rb.AddForce(rb.velocity.normalized * groundResistance * -1);
    }

    internal void updateWalkingVector()
    {
        Vector2 trackPadVector = SteamVR_Actions.player_controller.TP_left_vector.axis;
        Vector2 playerForward = new Vector2(playerHead.transform.forward.x, playerHead.transform.forward.z);
        Vector2 playerRight = new Vector2(playerHead.transform.right.x, playerHead.transform.right.z);
        Vector2 walkingVector = trackPadVector.y * playerForward + trackPadVector.x * playerRight;
        walkingVector3 = new Vector3(walkingVector.x, 0, walkingVector.y);
    }
}