﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateState : State
{
    public override void Start(StateMachine behaviour, object[] args = null)
    {
        //throw new System.NotImplementedException();
        Debug.Log("ENTERED SEARCH STATE");
    }

    public override void Update(StateMachine behaviour)
    {
        //throw new System.NotImplementedException();
    }

    public override void End(StateMachine behaviour)
    {
        //throw new System.NotImplementedException();
    }
}
