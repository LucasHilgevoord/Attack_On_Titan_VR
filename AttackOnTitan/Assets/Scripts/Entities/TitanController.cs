using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanController : StateMachine
{
    public enum TitanType
    {
        NORMAL,
        ABNORMAL,
        CRAWLER
    }

    public StateID[] states = {
        new StateID("WANDER", new WanderState()),
        new StateID("IDLE", new IdleState()),
        new StateID("SEARCH", new InvestigateState()),
        new StateID("CHASE", new ChaseState()),
        new StateID("ATTACK", new AttackState()),
        new StateID("DEAD", new DeadState())
    };

    public float intelligence = 1;
    public TitanType type;

    void Start()
    {
        switch (type)
        {
            case TitanType.NORMAL:
                intelligence = Random.Range(1, 5);
                break;
            case TitanType.ABNORMAL:
                intelligence = Random.Range(5, 10);
                break;
            case TitanType.CRAWLER:
                intelligence = Random.Range(5, 10);
                break;
            default:
                break;
        }
        for (int i = 0; i < states.Length; i++)
        {
            AddState(states[i].stateName, states[i].stateScript);
        }
        ChangeState("IDLE");
    }

    new void Update()
    {
        base.Update();
    }
}
