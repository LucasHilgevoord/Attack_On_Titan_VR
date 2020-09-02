using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanController : StateMachine
{
    public StateID[] states = {
        //new StateID("WANDER", typeof(WanderState), null),
        new StateID("IDLE", typeof(IdleState), null),
        new StateID("CHASE", typeof(ChaseState), null),
        new StateID("ATTACK", typeof(AttackState), null),
        //new StateID("DEAD", typeof(WanderState), null)
    };

    public TitanTypes.TitanType type;

    void Start()
    {
        //Adding all states
        for (int i = 0; i < states.Length; i++)
        {
            AddState(states[i].stateName, states[i].stateScript);
        }
        ChangeState("IDLE");
    }
}
