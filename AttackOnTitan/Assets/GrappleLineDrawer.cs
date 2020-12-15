using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleLineDrawer : MonoBehaviour
{

    LineRenderer grappleLine;
    //[SerializeField]
    float tensionConstant = 100;

    public AnimationCurve curveEffectCurve;
    public AnimationCurve timeEffect;
    public AnimationCurve curve;

    private float curveSize = 7;
    private float curveAmount = 0.3f;
    private float animationTime = 2;
    private float pullTime = 0.5f;

    public int lineSegments = 1000;

    // Start is called before the first frame update
    void Start()
    {
        grappleLine = GetComponent<LineRenderer>();
        grappleLine.positionCount = lineSegments;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawLooseLine(Vector3 startPos, Vector3 endPos, float time, float flatPart)
    {
        grappleLine.SetPosition(0, startPos);
        grappleLine.SetPosition(grappleLine.positionCount-1, endPos);

        //Calculate a parabola that will go through the lines
        Vector3 horizontalVec = endPos - startPos;
        horizontalVec.y = 0;

        for(int i = 1; i < grappleLine.positionCount-1; i++)
        {
            float ratio = ((float)i / grappleLine.positionCount);

            Vector3 displacement = Vector3.zero;

            displacement += curve.Evaluate(ratio * (endPos - startPos).magnitude / curveSize) * curveAmount * Vector3.Cross(horizontalVec, Vector3.up).normalized;
            displacement += curve.Evaluate(0.25f + ratio * (endPos - startPos).magnitude / curveSize) * curveAmount * Vector3.Cross(horizontalVec, Vector3.right).normalized;

            displacement *= curveEffectCurve.Evaluate(1 - ratio) * timeEffect.Evaluate(time / animationTime);

            //logistic flatPart
            displacement *= 1 / (1 + Mathf.Exp(tensionConstant * (ratio - 1 + flatPart / pullTime)));

            //displacement += (Mathf.Exp(ratio) - 1) * (cof * Mathf.Pow(ratio * horizontalVec.magnitude - Xzero, 2) + Yzero - ratio * heightDiff) * Vector3.up;

            grappleLine.SetPosition(i, startPos + ratio * (endPos - startPos) + displacement);
        }
    }
}
