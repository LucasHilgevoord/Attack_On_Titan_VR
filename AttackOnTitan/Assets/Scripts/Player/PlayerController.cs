using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerController : MonoBehaviour
{
    #region parameters
    [Header("References")]
    [SerializeField] private GameObject playerHead;
    private Rigidbody rb;
    private Collider col;
    [SerializeField] private RootMotion.FinalIK.VRIK vrik;
    [SerializeField] private Transform pelvis;
    [SerializeField] private Transform llmt; // Left leg movable target
    [SerializeField] private Transform rlmt; //right leg movable target
    [SerializeField] private Transform leftLegTarget;
    [SerializeField] private Transform rightLegTarget;

    [Header("(Optional) Time Controller")]
    [Tooltip("If a Time Controller is applied to the rigid body, add it here so the calculations will still be valid.")]
    [SerializeField] private TimeController timeController;

    [Header("Stats")]
    private float groundResistance = 3;
    private float walkingForce = 20;
    private float maxWalkSpeed = 5;

    [Header("Options")]
    [Tooltip("Continous rotation multiplier. The amount of degrees that you rotate per second at maximum speed.")]
    [SerializeField] private float contRotMultiplier = 100;
    [Tooltip("Snap rotation amount. You will always rotate this exact number of degrees in one snap.")]
    [SerializeField] private float snapRotAmount = 30;
    [Tooltip("Turn this on to use snap rotation, and off for continuous rotation.")]
    [SerializeField] private bool useSnapRotation = false;


    
    [Header("Calculation vars")]
    internal Vector3 walkingVector3;
    float groundDistance;
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

        //Player rotation
        if (useSnapRotation)
        {
            if (SteamVR_Actions.player_controller.TP_right_press.GetStateDown(SteamVR_Input_Sources.Any))
                RotatePlayer(Mathf.Sign(SteamVR_Actions.player_controller.TP_right_vector.axis.x) * snapRotAmount);
        }
        else
        {
            if (SteamVR_Actions.player_controller.TP_right_press.state)
                RotatePlayer(SteamVR_Actions.player_controller.TP_right_vector.axis.x * contRotMultiplier * Time.deltaTime);
        }
        
    }

    private void FixedUpdate()
    {
        groundDistance = DistToGround(10);
        if (groundDistance <= 0.1f)
        {
            WalkAndSlideForce(); //Note: This function will also manage IK
        }
        else
        {
            IK_Fly();
        }
    }

    internal bool IsGrounded()
    {
        float DistanceToTheGround = col.bounds.extents.y;
        return Physics.Raycast(transform.position + new Vector3(0, 0.25f, 0), Vector3.down, DistanceToTheGround + 0.1f);
    }

    internal float DistToGround(float maxRange)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, playerHead.transform.localPosition.y, 0), Vector3.down, out hit, maxRange, LayerMask.GetMask("Default")))
        {
            return hit.distance - playerHead.transform.localPosition.y;
        }
        else
        {
            return float.PositiveInfinity;
        }
    }

    private void WalkAndSlideForce()
    {
        UpdateWalkingVector();

        if (SteamVR_Actions.player_controller.TP_left_press.state && TimeFunctions.GetRealVelocity(timeController, rb).magnitude <= maxWalkSpeed * walkingVector3.magnitude)
        {
            rb.AddForce(walkingVector3 * walkingForce);
        }
        else
        {
            rb.AddForce(TimeFunctions.GetRealVelocity(timeController, rb).normalized * groundResistance * -1);
        }

        //for IK
        if (TimeFunctions.GetRealVelocity(timeController, rb).magnitude > maxWalkSpeed * 1.1f)
        {
            IK_Slide();
        }
        else
        {
            IK_Walk();
        }
    }

    internal void UpdateWalkingVector()
    {
        Vector2 trackPadVector = SteamVR_Actions.player_controller.TP_left_vector.axis;
        Vector2 playerForward = new Vector2(playerHead.transform.forward.x, playerHead.transform.forward.z);
        Vector2 playerRight = new Vector2(playerHead.transform.right.x, playerHead.transform.right.z);
        Vector2 walkingVector = trackPadVector.y * playerForward + trackPadVector.x * playerRight;
        walkingVector3 = new Vector3(walkingVector.x, 0, walkingVector.y);
    }

    internal void RotatePlayer(float degrees)
    {
        transform.Rotate(0, degrees, 0);

        if (groundDistance > 0.25f) //IK should rotate on it's own on ground, but not in mid-air
        {
            vrik.solver.AddPlatformMotion(Vector3.zero, Quaternion.Euler(0, degrees, 0), playerHead.transform.position);
        }
    }

    internal void ControlIK(float locoWeight, float posWeight, float rotWeight)
    {
        vrik.solver.locomotion.weight = locoWeight;// Mathf.Lerp(vrik.solver.locomotion.weight, locoWeight, 10 * TimeFunctions.DeltaTime(timeController));
        if (locoWeight == 0)
            vrik.solver.Reset();


        vrik.solver.leftLeg.positionWeight = Mathf.Lerp(vrik.solver.leftLeg.positionWeight, posWeight, 100 * TimeFunctions.DeltaTime(timeController));
        vrik.solver.rightLeg.positionWeight = Mathf.Lerp(vrik.solver.rightLeg.positionWeight, posWeight, 100 * TimeFunctions.DeltaTime(timeController));
        vrik.solver.leftLeg.rotationWeight = Mathf.Lerp(vrik.solver.leftLeg.rotationWeight, rotWeight, 100 * TimeFunctions.DeltaTime(timeController));
        vrik.solver.rightLeg.rotationWeight = Mathf.Lerp(vrik.solver.rightLeg.rotationWeight, rotWeight, 100 * TimeFunctions.DeltaTime(timeController));

        llmt.rotation = leftLegTarget.rotation;
        rlmt.rotation = rightLegTarget.rotation;
    }

    internal Vector3 leftLocalHip()
    {
        return -transform.InverseTransformDirection(pelvis.right) * 0.1f + new Vector3(pelvis.localPosition.x, 0, pelvis.localPosition.z);
    }

    internal Vector3 rightLocalHip()
    {
        return transform.InverseTransformDirection(pelvis.right) * 0.1f + new Vector3(pelvis.localPosition.x, 0, pelvis.localPosition.z);
    }

    internal Vector3 locRBvelocity()
    {
        return transform.InverseTransformDirection(TimeFunctions.GetRealVelocity(timeController, rb));
    }

    internal void IK_Walk()
    {
        ControlIK(1, 0, 0);
    }

    internal void IK_Slide()
    {
        Vector3 localRBvelocity = locRBvelocity();

        llmt.localPosition = new Vector3(localRBvelocity.x, 0, localRBvelocity.z).normalized
            * Mathf.Clamp(localRBvelocity.magnitude - maxWalkSpeed, 1, 10) * 0.05f + leftLocalHip();

        rlmt.localPosition = new Vector3(localRBvelocity.x, 0, localRBvelocity.z).normalized
            * Mathf.Clamp(localRBvelocity.magnitude - maxWalkSpeed, 1, 10) * 0.05f + rightLocalHip();

        ControlIK(0, 1, 0);
    }

    internal void IK_Fly()
    {
        float t = 0.1f / groundDistance;
        llmt.localPosition = Vector3.Lerp(transform.InverseTransformPoint(leftLegTarget.position), leftLocalHip() + (Vector3.down + locRBvelocity()).normalized, t);

        rlmt.localPosition = Vector3.Lerp(transform.InverseTransformPoint(rightLegTarget.position), rightLocalHip() + (Vector3.down + locRBvelocity()).normalized, t);

        vrik.solver.AddPlatformMotion(rb.velocity * TimeFunctions.DeltaTime(timeController), Quaternion.identity, playerHead.transform.position);
        ControlIK(0, 1, Mathf.Clamp01(1 - t));
    }
}