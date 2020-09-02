using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StateID
{
    public string stateName;
    public Type stateScript;
    public object[] args;
    public StateID(string name, Type script, object[] args = null)
    {
        stateName = name;
        stateScript = script;
        args = this.args;
    }
}

public class StateMachine : MonoBehaviour
{
    private Dictionary<string, Type> states = new Dictionary<string, Type>();
    private State currentState = null;

    /// <summary>
    /// Add a state to the StateMachine.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    protected void AddState(string id, Type state)
    {
        states.Add(id, state);
    }

    /// <summary>
    /// Change the current state to the assigned new state.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="args"></param>
    public void ChangeState(string id)
    {
        Destroy(currentState);
        if (!states.ContainsKey(id))
        {
            currentState = null;
            return;
        }
        currentState = this.gameObject.AddComponent(states[id]) as State;
    }
}
