using UnityEngine;
using System.Collections;

public abstract class State
{
    public abstract void Start(StateMachine behaviour, object[] args = null);
    public abstract void Update(StateMachine behaviour);
    public abstract void End(StateMachine behaviour);
}
