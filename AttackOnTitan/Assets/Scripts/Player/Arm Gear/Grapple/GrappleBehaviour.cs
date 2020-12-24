﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Script to control the movement of a grapple.
/// </summary>
public class GrappleBehaviour : MonoBehaviour
{
    [Header("Components/references")]
    [SerializeField] GameObject grapple;
    [SerializeField] Transform handTarget;
    [SerializeField] string definedButton = "";
    [SerializeField] GameObject targetSphere;
    [SerializeField] float targetSphereScale = 0.008f;
    [SerializeField] private AudioSource audioSource;

    [Header("Grapple properties")]
    [SerializeField] float grappleSpeed = 200;
    [SerializeField] float grappleLength = 50;

    [Header("(Optional) Time Controller")]
    [Tooltip("If a Time Controller is applied to the rigid body, add it here so the calculations will still be valid.")]
    [SerializeField] private TimeController timeController;

    private float timeSpeed = 1;



    bool pressedTrigger = false;
    bool canGrapple = true;

    private Valve.VR.SteamVR_Action_Single button;

    LineRenderer grappleLine;

    float lineTimer = 0;
    bool linePhase = false;
    float flatPart = 0;

    private void Start()
    {
        if (definedButton == "TriggerLeft") button = SteamVR_Actions.player_controller.trigger_left;
        if (definedButton == "TriggerRight") button = SteamVR_Actions.player_controller.trigger_right;

        grappleLine = GetComponent<LineRenderer>();

        grapple.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (timeController != null)
            timeSpeed = timeController.TimeSpeed;

        if(button.axis == 1f)
        {
            pressedTrigger = true;
            if (canGrapple && button.changed)
            {
                startGrapple();
                canGrapple = false;
            }
        }
        else pressedTrigger = false;

        //Make the LineRenderer connect grapple and grapple point.
        if (grappleLine.enabled)
        {
            GetComponent<GrappleLineDrawer>().DrawLooseLine(grapple.transform.position, transform.position, lineTimer, flatPart);
            lineTimer += TimeFunctions.DeltaTime(timeController);

            if (linePhase)
            {
                flatPart += TimeFunctions.DeltaTime(timeController);
            }
        }

        //Draw Circle at target
        if (grapple.active == false)
        {
            RaycastHit target;
            if(Physics.Raycast(handTarget.position, handTarget.up,
                out target, grappleLength, LayerMask.GetMask("Default")))
            {
                targetSphere.transform.position = target.point;
                targetSphere.SetActive(true);
                targetSphere.transform.localScale = target.distance * targetSphereScale * Vector3.one;
            }
            else
            {
                targetSphere.SetActive(false);
            }
        }
        else
        {
            targetSphere.SetActive(false);
        }
    }

    void startGrapple()
    {
        grapple.SetActive(true);
        grapple.transform.position = transform.position;
        grapple.transform.LookAt(grapple.transform.position + handTarget.up);
        grappleLine.enabled = true;
        lineTimer = 0;
        flatPart = 0;
        StartCoroutine(MoveGrapple(grapple.transform));
    }

    void endGrapple()
    {
        grapple.SetActive(false);
        grapple.transform.parent = null;
    }

    IEnumerator MoveGrapple(Transform grapple)
    {
        while (pressedTrigger && Vector3.Distance(grapple.position, transform.position) <= grappleLength)
        {
            //Move foward
            if (!grapple.GetComponent<HookBehavior>().collided)
            {
                Vector3 velocityVector = grapple.transform.forward * grappleSpeed;
                RaycastHit grappleInfo;
                if (Physics.Raycast(grapple.position, velocityVector, out grappleInfo, grappleSpeed * TimeFunctions.DeltaTime(timeController), LayerMask.GetMask("Default")))
                {
                    //The grapple is hitting something in this frame, so instead of updating the position, we immediatly do the collision code
                    grapple.position = grappleInfo.point;
                    grapple.GetComponent<HookBehavior>().collided = true;
                    grapple.GetComponent<Collider>().enabled = false;
                    GameObject empty = new GameObject();
                    empty.name = "Grapple Parent";
                    empty.transform.parent = grappleInfo.collider.gameObject.transform;
                    grapple.parent = empty.transform;
                    grapple.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    linePhase = true;
                    lineTimer = 0;
                }
                else
                {
                    //Everything is clear, the grapple goes further
                    grapple.position += velocityVector * TimeFunctions.DeltaTime(timeController);
                }

            }
            else if (grapple.parent == null)
            {
                //Trick to make grapple child of object without skewing the scale
                GameObject empty = new GameObject();
                empty.name = "Grapple Parent";
                empty.transform.parent = grapple.GetComponent<HookBehavior>().collidedObject.transform;
                grapple.parent = empty.transform;
                grapple.GetComponent<Rigidbody>().velocity = Vector3.zero;
                linePhase = true;
                lineTimer = 0;
            }
            yield return new WaitForEndOfFrame();
        }

        //If button is released or grapples reached max distance, move the grapple back
        StartCoroutine(MoveGrappleBack(grapple.transform));
    }

    IEnumerator MoveGrappleBack(Transform grapple)
    {
        //grapple.GetComponent<Rigidbody>().isKinematic = false;
        grapple.GetComponent<HookBehavior>().collided = false;
        if(grapple.parent != null)
        {
            Destroy(grapple.parent.gameObject);
        }
        grapple.parent = null;

        //Manage grapple line
        linePhase = true;
        lineTimer = 0;

        while (Vector3.Distance(grapple.position, transform.position) > grappleSpeed * TimeFunctions.DeltaTime(timeController) + 0.5f)
        {
            grapple.position += (transform.position - grapple.position).normalized * grappleSpeed * TimeFunctions.DeltaTime(timeController);
            yield return new WaitForEndOfFrame();
        }

        //Allow grappling again
        endGrapple();
        canGrapple = true;
        grappleLine.enabled = false;
        linePhase = false;
    }
}
