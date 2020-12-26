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
    [SerializeField] private RootMotion.FinalIK.VRIK vrik;
    [SerializeField] private Transform pelvis; //Note: refers to pelvisParent, not pelvisTarget;
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
    private float rotationMultiplier = 100;
    
    [Header("Calculation vars")]
    internal Vector3 walkingVector3;
    private Vector2 rotationVector;
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
    }

    private void FixedUpdate()
    {
        groundDistance = DistToGround(10);

        //player rotation
        if (SteamVR_Actions.player_controller.TP_right_press.state)
        {
            rotationVector = SteamVR_Actions.player_controller.TP_right_vector.axis;
            transform.Rotate(0, rotationVector.x * rotationMultiplier * Time.deltaTime, 0);

            if(groundDistance > 0.25f) //IK should rotate on it's own on ground, but not in mid-air
            {
                vrik.solver.AddPlatformMotion(rb.velocity * TimeFunctions.DeltaTime(timeController),
                Quaternion.Euler(0, rotationVector.x * rotationMultiplier * Time.deltaTime, 0), playerHead.transform.position);
            }
        }

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

        //vrik.solver.Reset();
        ControlIK(0, 1, 0);
    }

    internal void IK_Fly()
    {
        float t = 0.1f / groundDistance;
        llmt.localPosition = Vector3.Lerp(transform.InverseTransformPoint(leftLegTarget.position), leftLocalHip() + (Vector3.down + locRBvelocity()).normalized, t);

        rlmt.localPosition = Vector3.Lerp(transform.InverseTransformPoint(rightLegTarget.position), rightLocalHip() + (Vector3.down + locRBvelocity()).normalized, t);

        vrik.solver.AddPlatformMotion(rb.velocity * TimeFunctions.DeltaTime(timeController), Quaternion.identity, playerHead.transform.position);
        //vrik.solver.Reset();
        ControlIK(0, 1, Mathf.Clamp01(1 - t));
    }
}