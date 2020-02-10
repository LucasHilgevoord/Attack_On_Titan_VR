using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : State
{
    private GameObject target;
    private NavMeshAgent agent;

    private float currentVelocity;
    private float walkVelocity = 5;
    private float runVelocity = 10;
    private float multiplier = 1;

    public override void Start(StateMachine behaviour, object[] args = null)
    {
        target = (GameObject)args[0];
        agent = behaviour.GetComponent<NavMeshAgent>();

        TitanController controller = (TitanController)behaviour;
        multiplier = controller.intelligence;
        Debug.Log(multiplier);

        //throw new System.NotImplementedException();
    }

    public override void Update(StateMachine behaviour)
    {
        agent.destination = target.transform.position;
        //throw new System.NotImplementedException();
    }

    public override void End(StateMachine behaviour)
    {
        //throw new System.NotImplementedException();
    }
}
