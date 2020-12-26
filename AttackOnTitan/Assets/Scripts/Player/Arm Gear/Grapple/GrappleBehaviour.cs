using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Script to control the movement of a grapple.
/// </summary>
public class GrappleBehaviour : MonoBehaviour
{
    [Header("Components/references")]
    [SerializeField] public GameObject grapple;
    [SerializeField] Transform handTarget;
    [Tooltip("Defined button should be either 'TriggerLeft' or 'TriggerRight'")]
    [SerializeField] string definedButton = "";
    [SerializeField] GameObject targetSphere;
    [SerializeField] float targetSphereScale = 0.008f;
    [SerializeField] private AudioSource audioSource;

    [Header("Grapple properties")]
    [SerializeField] float grappleSpeed = 200;
    [Tooltip("Length of the rope, or in other words, the range of the grapple.")]
    [SerializeField] float grappleLength = 50;

    [Header("(Optional) Time Controller")]
    [Tooltip("If a Time Controller is applied to the rigid body, add it here so the calculations will still be valid.")]
    [SerializeField] private TimeController timeController;

    internal bool collided = false;

    bool pressedTrigger = false;
    bool canGrapple = true;

    private Valve.VR.SteamVR_Action_Single button;

    LineRenderer grappleLine;
    GrappleLineDrawer lineDrawer;

    private void Start()
    {
        if (definedButton == "TriggerLeft") button = SteamVR_Actions.player_controller.trigger_left;
        if (definedButton == "TriggerRight") button = SteamVR_Actions.player_controller.trigger_right;

        grappleLine = GetComponent<LineRenderer>();
        lineDrawer = GetComponent<GrappleLineDrawer>();

        grapple.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if(button.axis == 1f)
        {
            pressedTrigger = true;
            if (canGrapple && button.changed)
            {
                startGrapple();
            }
        }
        else pressedTrigger = false;

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
        canGrapple = false;
        grapple.SetActive(true);
        grapple.transform.position = transform.position;
        grapple.transform.LookAt(grapple.transform.position + handTarget.up);
        grappleLine.enabled = true;
        lineDrawer.StartGrappleLine();
        StartCoroutine(MoveGrapple(grapple));

        audioSource.Play();
    }

    void endGrapple()
    {
        canGrapple = true;
        lineDrawer.EndGrappleLine();
        grapple.SetActive(false);
        grapple.transform.parent = null;
    }

    IEnumerator MoveGrapple(GameObject grapple)
    {
        while (pressedTrigger && Vector3.Distance(grapple.transform.position, transform.position) <= grappleLength)
        {
            //Move foward
            if (!collided)
            {
                Vector3 velocityVector = grapple.transform.forward * grappleSpeed;
                RaycastHit grappleInfo;
                if (Physics.Raycast(grapple.transform.position, velocityVector, out grappleInfo, grappleSpeed * TimeFunctions.DeltaTime(timeController), LayerMask.GetMask("Default")))
                {
                    //The grapple is hitting something in this frame
                    grapple.transform.position = grappleInfo.point;
                    collided = true;
                    GameObject empty = new GameObject();
                    empty.name = "Grapple Parent";
                    empty.transform.parent = grappleInfo.collider.gameObject.transform;
                    grapple.transform.parent = empty.transform;
                    lineDrawer.TightenGrappleLine();
                }
                else
                {
                    //Everything is clear, the grapple goes further
                    grapple.transform.position += velocityVector * TimeFunctions.DeltaTime(timeController);
                }

            }
            yield return new WaitForEndOfFrame();
        }

        //If button is released or grapples reached max distance, move the grapple back
        StartCoroutine(MoveGrappleBack(grapple.transform));
    }

    IEnumerator MoveGrappleBack(Transform grapple)
    {
        collided = false;
        if(grapple.parent != null)
        {
            Destroy(grapple.parent.gameObject);
        }
        grapple.parent = null;

        while (Vector3.Distance(grapple.position, transform.position) > grappleSpeed * TimeFunctions.DeltaTime(timeController) + 0.5f)
        {
            grapple.position += (transform.position - grapple.position).normalized * grappleSpeed * TimeFunctions.DeltaTime(timeController);
            yield return new WaitForEndOfFrame();
        }

        //Allow grappling again
        endGrapple();
    }
}
