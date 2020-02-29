using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanController : StateMachine
{
    public StateID[] states = {
        new StateID("WANDER", new WanderState()),
        new StateID("IDLE", new IdleState()),
        new StateID("SEARCH", new InvestigateState()),
        new StateID("CHASE", new TitanChase()),
        new StateID("ATTACK", new AttackState()),
        new StateID("DEAD", new DeadState())
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

    new void Update()
    {
        //Updating the Update in StateMachine
        base.Update();
    }
}
