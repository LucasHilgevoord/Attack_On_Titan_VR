using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    private float minIdleTimer = 5f;
    private float maxIdleTimer = 15f;
    private float currentTimer;

    private void Start()
    {
        currentTimer = Random.Range(minIdleTimer, maxIdleTimer);
    }

    private void Update()
    {
        //Debug.LogFormat("<b><color=green>Idle State:</color> {0}</b>", "Timer: " + (int)currentTimer);
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            controller.ChangeState("WANDER");
        }
    }

    public override void OnDestroy()
    {
        //throw new System.NotImplementedException();
    }
}
