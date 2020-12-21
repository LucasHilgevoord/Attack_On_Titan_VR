using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grapple
{
    [SerializeField] private string name;

    [Tooltip("With 'hand' I originally intended the controller, but any child of that (like handTarget) should work too")]
    public GameObject hand;
    [Tooltip("The actual grapple itself, aka, the hook")]
    public HookBehavior grapple;

    //grappleVec is the normalized vector pointing from the player to the grapple
    internal Vector3 grappleVec;

    //Vars for activating strong pull
    internal float pullCounter;
    internal bool strongPull;

    internal Vector3 handPosition;
    internal Vector3 prevHandPosition;
    internal Vector3 beforePrevHandPosition;
    internal Vector3 relativeDisplacement;
    internal Vector3 prevRelativeDisplacement;
    internal float speedDifferenceProjection;

    [Tooltip("This is related with how hard as well as how far you pull")]
    public float pullCounterTotalThreshold = 3f;
    [Tooltip("Minimum acceleration of hands to register 'being pulled strongly'")]
    public float pullCounterAccThreshold = 5; //Minimum acceleration of hands to activate the strong pull
}
