using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleLineDrawer : MonoBehaviour
{

    LineRenderer grappleLine;
    [SerializeField] Transform grapple;
    [SerializeField] Transform grapplePoint;

    //[SerializeField]
    float tensionConstant = 100;
    float spiralSpeed = 1;

    public AnimationCurve curveEffectCurve;
    public AnimationCurve timeEffect;
    public AnimationCurve curve;

    private float curveSize = 7;
    private float curveAmount = 0.3f;
    private float animationTime = 2;
    private float pullTime = 0.5f;

    public int lineSegments = 1000;

    [Header("(Optional) Time Controller")]
    [Tooltip("If a Time Controller is applied to the rigid body, add it here so the calculations will still be valid.")]
    [SerializeField] private TimeController tc;

    int linePhase = 0;
    float timer = 0;
    float flatPart = 0;

    // Start is called before the first frame update
    void Start()
    {
        grappleLine = GetComponent<LineRenderer>();
        grappleLine.positionCount = lineSegments;
    }

    private void Update()
    {
        switch (linePhase)
        {
            case 1:
                DrawLooseLine(grapple.position, grapplePoint.position, timer, 0);
                timer += TimeFunctions.DeltaTime(tc);
                break;
            case 2:
                DrawLooseLine(grapple.position, grapplePoint.position, timer, flatPart);
                timer += TimeFunctions.DeltaTime(tc);
                flatPart += TimeFunctions.DeltaTime(tc);
                break;
        }
    }

    public void StartGrappleLine()
    {
        linePhase = 1;
        timer = 0;
        grappleLine.enabled = true;
    }

    public void TightenGrappleLine()
    {
        linePhase = 2;
        flatPart = 0;
    }

    public void EndGrappleLine()
    {
        linePhase = 0;
        grappleLine.enabled = false;
    }

    public void DrawLooseLine(Vector3 startPos, Vector3 endPos, float time, float flatPart)
    {
        grappleLine.SetPosition(0, startPos);
        grappleLine.SetPosition(grappleLine.positionCount - 1, endPos);

        //Calculate a parabola that will go through the lines
        Vector3 horizontalVec = endPos - startPos;
        horizontalVec.y = 0;

        for (int i = 1; i < grappleLine.positionCount - 1; i++)
        {
            float ratio = ((float)i / grappleLine.positionCount);

            Vector3 displacement = Vector3.zero;

            float progress = (1-ratio) * (endPos - startPos).magnitude / curveSize;
            displacement += curve.Evaluate(spiralSpeed * time + progress) * curveAmount * Vector3.Cross(horizontalVec, Vector3.up).normalized;
            displacement += curve.Evaluate(0.25f + spiralSpeed * time + progress) * curveAmount * Vector3.Cross(horizontalVec, Vector3.right).normalized;

            displacement *= curveEffectCurve.Evaluate(1 - ratio) * timeEffect.Evaluate(time / animationTime);

            //logistic flatPart
            displacement *= 1 / (1 + Mathf.Exp(tensionConstant * (ratio - 1 + flatPart / pullTime)));

            grappleLine.SetPosition(i, startPos + ratio * (endPos - startPos) + displacement);
        }
    }
}
