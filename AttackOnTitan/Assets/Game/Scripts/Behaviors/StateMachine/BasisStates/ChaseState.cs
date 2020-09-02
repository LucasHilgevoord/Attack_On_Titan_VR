using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    public Rigidbody rb;
    public GameObject target;

    public Vector3 curVelocity;
    private Vector3 desiredVelocity;
    public float maxVelocity = 2;
    public float maxForce = 5;
    public float mass;

    public void Start()
    {
        rb = controller.gameObject.GetComponent<Rigidbody>();
        //target = (GameObject)args[0];
        curVelocity = Vector3.zero;
        //throw new System.NotImplementedException();
    }

    private void Update()
    {
        FollowPlayer();
        //agent.destination = target.transform.position;
        //throw new System.NotImplementedException();
    }

    public override void OnDestroy()
    {
        //throw new System.NotImplementedException();
    }

    private void FollowPlayer()
    {
        Vector3 controllerPos = controller.transform.position;
        var desiredVelocity = target.transform.position - controllerPos;
        desiredVelocity = desiredVelocity.normalized * maxVelocity;

        var steering = desiredVelocity - curVelocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= rb.mass;

        curVelocity = Vector3.ClampMagnitude(curVelocity + steering, maxVelocity);
        controllerPos += curVelocity * Time.deltaTime;
        controller.transform.forward = new Vector3(curVelocity.normalized.x, 0, curVelocity.normalized.z);

        Debug.DrawRay(controllerPos, curVelocity.normalized * 2, Color.green);
        Debug.DrawRay(controllerPos, desiredVelocity.normalized * 2, Color.magenta);
    }
}
