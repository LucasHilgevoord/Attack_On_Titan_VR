using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanChase : ChaseState
{
    private TitanTypes.TitanType type;

    public float maxWalkVelocity = 2;
    public float maxRunVelocity = 10;

    private void Start()
    {
        base.Start();

        controller = (TitanController)controller;
        type = controller.type;

        //Setting speed
        if (type == TitanTypes.TitanType.NORMAL)
            maxVelocity = maxWalkVelocity;
        else
            maxVelocity = maxRunVelocity;
    }
}
