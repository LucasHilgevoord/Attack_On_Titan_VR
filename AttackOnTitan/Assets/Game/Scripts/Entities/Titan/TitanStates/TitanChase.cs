using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanChase : ChaseState
{
    private TitanTypes.TitanType type;

    public float maxWalkVelocity = 2;
    public float maxRunVelocity = 10;

    public override void Start(StateMachine behaviour, object[] args = null)
    {
        base.Start(behaviour, args);

        TitanController controller = (TitanController)behaviour;
        type = controller.type;

        //Setting speed
        if (type == TitanTypes.TitanType.NORMAL)
            maxVelocity = maxWalkVelocity;
        else
            maxVelocity = maxRunVelocity;
    }
}
