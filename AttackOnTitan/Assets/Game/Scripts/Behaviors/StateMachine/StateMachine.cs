using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateID
{
    public string stateName;
    public State stateScript;
    public StateID(string name, State script)
    {
        stateName = name;
        stateScript = script;
    }
}

public class StateMachine : MonoBehaviour
{
    private State currentState = null;
    private Dictionary<string, State> states = new Dictionary<string, State>();

    protected void Update()
    {
        if (currentState != null) currentState.Update();
    }

    protected void AddState(string id, State state)
    {
        states.Add(id, state);
        Debug.LogFormat("<b><color=blue>State added:</color> {0}</b>", id);
    }

    public void ChangeState(string id, object[] args = null)
    {
        if (currentState != null) currentState.End();
        if (!states.ContainsKey(id))
        {
            currentState = null;
            return;
        }
        currentState = states[id];
        currentState.Start(this, args);
        Debug.LogFormat("<b><color=red>State started:</color> {0}</b>", id);
    }
}
