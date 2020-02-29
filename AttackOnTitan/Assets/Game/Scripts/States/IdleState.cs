using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    private StateMachine controller;
    private float minIdleTimer = 5f;
    private float maxIdleTimer = 15f;
    private float currentTimer;

    public override void Start(StateMachine _controller, object[] _args = null)
    {
        controller = _controller;
        currentTimer = Random.Range(minIdleTimer, maxIdleTimer);
    }

    public override void Update()
    {
        //Debug.LogFormat("<b><color=green>Idle State:</color> {0}</b>", "Timer: " + (int)currentTimer);
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            controller.ChangeState("WANDER");
        }
    }

    public override void End()
    {
        //throw new System.NotImplementedException();
    }
}
