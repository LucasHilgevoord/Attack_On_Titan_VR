using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GrappelingController : MonoBehaviour
{

    [SerializeField]
    GameObject sword;

    [SerializeField]
    GameObject grapplePrefab;
    [SerializeField]
    float grappleSpeed = 200;
    [SerializeField]
    float grappleLength = 50;

    [SerializeField]
    string definedButton = "";

    bool pressedTrigger = false;
    bool canGrapple = true;

    private Valve.VR.SteamVR_Action_Single button;

    private void Start()
    {
        if (definedButton == "TriggerLeft") button = SteamVR_Actions.player_controller.trigger_left;
        if (definedButton == "TriggerRight") button = SteamVR_Actions.player_controller.trigger_right;
    }


    // Update is called once per frame
    void Update()
    {
        if(button.axis > 0.5f)
        {
            pressedTrigger = true;
            if (canGrapple) CreateGrapple();
            canGrapple = false;
        }
        else pressedTrigger = false;
    }

    void CreateGrapple()
    {
        GameObject grapple = Instantiate(grapplePrefab);
        grapple.name = "Grapple";
        grapple.transform.position = transform.position;
        grapple.transform.rotation = sword.transform.rotation;
        StartCoroutine(MoveGrapple(grapple.transform, GetComponentInParent<Rigidbody>().velocity));
    }

    IEnumerator MoveGrapple(Transform grapple, Vector3 beginVelocity)
    {
        while (pressedTrigger && Vector3.Distance(grapple.position, transform.position) <= grappleLength)
        {
            //Move foward
            if (!grapple.GetComponent<HookBehavior>().collided)
            {
                Vector3 velocityVector = grapple.transform.forward * grappleSpeed + beginVelocity;
                grapple.position += velocityVector * Time.deltaTime;
                RaycastHit grappleInfo;
                if(Physics.Raycast(grapple.position, velocityVector, out grappleInfo, grappleSpeed * Time.deltaTime))
                {
                    grapple.position += velocityVector.normalized * (grappleInfo.distance + 0.5f);
                }
                
            }
            else
            {
                grapple.parent = grapple.GetComponent<HookBehavior>().collidedObject.transform;
                grapple.GetComponent<Rigidbody>().velocity = Vector3.zero;
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
        grapple.parent = null;
        Vector3 grappleVectorSpeed;
        while (Vector3.Distance(grapple.position, transform.position) > 0.5f)
        {
            grappleVectorSpeed = (transform.position - grapple.position).normalized * grappleSpeed;
            grapple.position += grappleVectorSpeed * Time.deltaTime;
            RaycastHit grappleInfo;
            Ray grappleRay = new Ray();
            grappleRay.origin = grapple.position;
            grappleRay.direction = grappleVectorSpeed;

            GetComponent<Collider>().enabled = true;
            if (GetComponent<Collider>().Raycast(grappleRay, out grappleInfo, grappleSpeed * Time.deltaTime))
            {
                GetComponent<Collider>().enabled = false;
                grapple.position += grappleVectorSpeed.normalized * grappleInfo.distance;
            }
            yield return new WaitForEndOfFrame();
        }

        //Allow grappling again
        Destroy(grapple.gameObject);
        canGrapple = true;
    }
}
