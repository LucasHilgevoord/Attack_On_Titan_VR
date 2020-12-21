using UnityEngine;

/// <summary>
/// Script to control the states of the grapples.
/// </summary>
public class GrappleController : MonoBehaviour
{
    #region parameters
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Grapple leftGrapple;
    [SerializeField] private Grapple rightGrapple;

    [Header("Booleans")]
    private bool canGrappleJump = true;
    private bool useGravityCompensation;
    private bool warmUp = false;

    [Header("Forces")]
    private float jumpForce = 60;
    private float maxPullForce = 30;
    private float currentPullForce = 0;
    private float strongPullForce = 30;
    private float breakForce = 50;
    private float gravityCompensationForce = 3;
    private float gasAssistForce = 3;

    [Header("Resistance")]
    private float pullResistance = 2;
    private float gasAssistClampFactor = 0.1f;
    private float warmpUpSpeed = 50;
    #endregion

    private void Update()
    {
        CheckHardPull(leftGrapple);
        CheckHardPull(rightGrapple);
    }

    private void FixedUpdate()
    {
        warmUp = false;
        GrapplePullForce(leftGrapple);
        GrapplePullForce(rightGrapple);
        WarmUpGrapple();
    }

    /// <summary>
    /// Method to check if the player pulls hard enough to push the player towards the grapple.
    /// Also, the hand positions are updated here, so make sure this is always called in Update()
    /// </summary>
    private void CheckHardPull(Grapple grapple)
    {
        grapple.handPosition = grapple.hand.transform.localPosition;

        grapple.relativeDisplacement = grapple.handPosition - grapple.prevHandPosition;
        grapple.prevRelativeDisplacement = grapple.prevHandPosition - grapple.beforePrevHandPosition;

        grapple.beforePrevHandPosition = grapple.prevHandPosition;
        grapple.prevHandPosition = grapple.handPosition;

        if (grapple.grapple.collided)
        {
            //Calculate difference in speed and compute the projection of that onto grappleVec. This will be related to acceleration to/from the grapple
            grapple.speedDifferenceProjection = Vector3.Dot((grapple.relativeDisplacement - grapple.prevRelativeDisplacement) / Time.deltaTime, grapple.grappleVec);

            //Note: difference in speed = acceleration times delta time
            if (grapple.speedDifferenceProjection < -grapple.pullCounterAccThreshold * Time.deltaTime)
                grapple.pullCounter += -grapple.speedDifferenceProjection;
            else
                grapple.pullCounter = 0;

            if (grapple.pullCounter > grapple.pullCounterTotalThreshold)
                grapple.strongPull = true;
        }
    }

    /// <summary>
    /// This manages the forces coming from grapples. Should be called each frame in FixedUpdate()
    /// </summary>
    /// <param name="grapple"></param>
    internal void GrapplePullForce(Grapple grapple)
    {
        useGravityCompensation = false;
        if (grapple.grapple.collided)
        {
            MoveTowardGrapple(grapple);
            AddGasAssist(grapple);
            Jump(grapple);

            //Strong pull
            if (grapple.strongPull)
                rb.AddForce(grapple.grappleVec * strongPullForce);
        }
        else
            grapple.strongPull = false;

        //anti-gravity compensation with gas
        if (useGravityCompensation)
            rb.AddForce(new Vector3(0, 1, 0) * gravityCompensationForce);

    }

    /// <summary>
    /// Manages warming up and cooling down grapples.
    /// Bool warmUp should be set to false at the beginning of each frame and might be set to true by other functions. At the end of the frame, this function is called.
    /// </summary>
    /// <param name="grapple"></param>
    private void WarmUpGrapple()
    {
        if (warmUp)
            currentPullForce += warmpUpSpeed * Time.deltaTime;
        else
        {
            //grapple cooling down
            currentPullForce -= warmpUpSpeed * Time.deltaTime;
            if (currentPullForce < 0) currentPullForce = 0;
        }
    }

    /// <summary>
    /// Method to apply the main force, thus the force that actually pulls you towards the grapple.
    /// </summary>
    /// <param name="grapple"></param>
    private void MoveTowardGrapple(Grapple grapple)
    {
        //grapple warming up
        warmUp = true;

        if (currentPullForce > maxPullForce) { currentPullForce = maxPullForce; }

        grapple.grappleVec = (grapple.grapple.transform.position - transform.position).normalized;

        //Resistance
        float angle = Vector3.Angle(grapple.grappleVec, rb.velocity);
        float velocityToGrapple = rb.velocity.magnitude * Mathf.Cos((angle * Mathf.PI) / 180);

        //Forces
        rb.AddForce(grapple.grappleVec * currentPullForce); //Main force

        if (angle > 90)
            rb.AddForce(grapple.grappleVec * breakForce * velocityToGrapple * -1); //Grapple is trying to stop player from going away
        else
            rb.AddForce(grapple.grappleVec * pullResistance * velocityToGrapple * -1); //Grapple succesfully pulls the player but there is internal resistance
    }

    /// <summary>
    /// This method makes the player move more in a straight line towards the grapple.
    /// The trade-off is having more control but less speed, especially while swinging around the grapple
    /// </summary>
    /// <param name="grapple"></param>
    private void AddGasAssist(Grapple grapple)
    {

        //Gas Assist (makes player move more straight in the direction of the grapple and less around it)
        Vector3 velVec = rb.velocity.normalized;
        Vector3 gasAssistVec = -1 * velVec + Vector3.Dot(grapple.grappleVec, velVec) * grapple.grappleVec;
        rb.AddForce(gasAssistVec * gasAssistForce * Mathf.Clamp01(rb.velocity.magnitude * gasAssistClampFactor));
    }

    /// <summary>
    /// Make the player jump when it is grounded.
    /// </summary>
    /// <param name="grapple"></param>
    private void Jump(Grapple grapple)
    {
        //begin jump
        if (playerController.IsGrounded() && canGrappleJump)
        {
            rb.AddForce(new Vector3(0, 1, 0) * jumpForce); //Jump
            canGrappleJump = false;
        }
        if (!playerController.IsGrounded() && canGrappleJump) canGrappleJump = false;

        //for gravity compensation
        useGravityCompensation = true;
    }
}
