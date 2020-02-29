using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    public Transform controller;
    public Rigidbody rb;
    public GameObject target;

    public Vector3 curVelocity;
    private Vector3 desiredVelocity;
    public float maxVelocity = 2;
    public float maxForce = 5;
    public float mass;

    public override void Start(StateMachine _controller, object[] args = null)
    {
        controller = _controller.gameObject.transform;
        rb = _controller.gameObject.GetComponent<Rigidbody>();
        target = (GameObject)args[0];
        curVelocity = Vector3.zero;
        //throw new System.NotImplementedException();
    }

    public override void Update()
    {
        FollowPlayer();
        //agent.destination = target.transform.position;
        //throw new System.NotImplementedException();
    }

    public override void End()
    {
        //throw new System.NotImplementedException();
    }

    private void FollowPlayer()
    {
        var desiredVelocity = target.transform.position - controller.position;
        desiredVelocity = desiredVelocity.normalized * maxVelocity;

        var steering = desiredVelocity - curVelocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= rb.mass;

        curVelocity = Vector3.ClampMagnitude(curVelocity + steering, maxVelocity);
        controller.position += curVelocity * Time.deltaTime;
        controller.forward = new Vector3(curVelocity.normalized.x, 0, curVelocity.normalized.z);

        Debug.DrawRay(controller.position,curVelocity.normalized * 2, Color.green);
        Debug.DrawRay(controller.position, desiredVelocity.normalized * 2, Color.magenta);
    }
}
