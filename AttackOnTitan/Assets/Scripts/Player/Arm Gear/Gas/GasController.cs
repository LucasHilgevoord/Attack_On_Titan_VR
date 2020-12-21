using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GasController : MonoBehaviour
{
    #region parameters
    [Header("Components")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Stats")]
    [Tooltip("The force of each gas thruster. Using both hands will result in two separate forces with this magnitude")]
    [SerializeField] private float gasForce = 7;
    [Tooltip("Gas burst gives an instant velocity change of this magnitude")]
    [SerializeField] private float gasBurstSpeed = 1;
    [Tooltip("The volume of the gas tank in seconds of use. Using both hands means gas will deplete 2x faster")]
    [SerializeField] private float maxGas = 10000;
    [Tooltip("How much gas the gas burst uses")]
    [SerializeField] private float gasBurstCost = 5; //Amount of gas that the gas burst uses

    [Header("Options")]
    [Tooltip("False: Gas comes from palms.\nTrue: Kinda like a jetpack, gas is controlled with left trackpad")]
    [SerializeField] private bool altGasMode = false; //False (alternative gas mode): Gas comes from hands. true (normal gas mode): gas comes from palms
    [Tooltip("If gas is activated 2 times within this time apart, gas burst activates (seperate for both hands)")]
    [SerializeField] private float burstActivationTime = 0.5f;

    [Header("Calculation booleans")]
    private bool activateGasLeft = false;
    private bool activateGasRight = false;
    private bool activateGasBurstLeft = false;
    private bool activateGasBurstRight = false;

    [Header("Calculation floats")]
    [Tooltip("Current amount of gas (see Max Gas)")]
    [SerializeField] private float currentGas = 10000;
    private float gasTankParameter = 10; //The higher this is, the more "square" the graph looks of force vs tank content
    private float[] burstTimer = { 0, 0 };
    #endregion

    // Update is called once per frame
    private void Update()
    {
        if (SteamVR_Actions.player_controller.grip_left.state)
        {
            activateGasLeft = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasLeft = false;
        }

        if (SteamVR_Actions.player_controller.grip_right.state)
        {
            activateGasRight = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasRight = false;
        }

        //Manage gas burst
        if (SteamVR_Actions.player_controller.grip_left.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (burstTimer[0] > 0)
            {
                activateGasBurstLeft = true;
                burstTimer[0] = 0;
            }
            else burstTimer[0] = burstActivationTime;
        }
        burstTimer[0] -= Time.deltaTime;
        if (SteamVR_Actions.player_controller.grip_right.GetStateDown(SteamVR_Input_Sources.Any))
        {
            if (burstTimer[1] > 0)
            {
                activateGasBurstRight = true;
                burstTimer[1] = 0;
            }
            else burstTimer[1] = burstActivationTime;
        }
        burstTimer[1] -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (altGasMode)
        {
            if (activateGasLeft || activateGasRight) altUseGas();
        }
        else
        {
            if (activateGasLeft) useGas(leftHandTarget, false, 0); //Gas left side
            if (activateGasRight) useGas(rightHandTarget, false, 1); //Gas right side
            if (activateGasBurstLeft)
            {
                useGas(leftHandTarget, true, 0); //Gas burst left
                activateGasBurstLeft = false;
            }
            if (activateGasBurstRight)
            {
                useGas(rightHandTarget, true, 1); //Gas burst right
                activateGasBurstRight = false;
            }
        }
    }

    private void useGas(Transform hand, bool burst, int flip)
    {
        //Code to weaken force when tank is almost empty
        float tankRatio = currentGas / maxGas;
        float amplitude = 1 / (1 - Mathf.Exp(-gasTankParameter));
        float strength = gasForce * amplitude * (1 - Mathf.Exp(-gasTankParameter * tankRatio));

        if (burst)
        {
            rb.velocity += hand.right * Mathf.Pow(-1, flip) * gasBurstSpeed * strength; //same as below, but add a specific speed instantly
            currentGas -= gasBurstCost;
            if (currentGas < 0) currentGas = 0;
        }
        else
        {
            rb.AddForce(hand.right * Mathf.Pow(-1, flip) * strength); //Note: side=0 is left, side=1 is right
            //Also note: For the right hand (side=1) I need the right vector flipped, that's where the power of -1 is for.
            currentGas -= Time.deltaTime;
            if (currentGas < 0) currentGas = 0;
        }
    }

    private void altUseGas()
    {
        //Note: It kinda works but is not stable yet
        playerController.updateWalkingVector();
        float vectorHeight = Mathf.Sqrt(1 - playerController.walkingVector3.sqrMagnitude);
        Vector3 gasVector = playerController.walkingVector3 + new Vector3(0, vectorHeight, 0);

        float tankRatio = currentGas / maxGas;
        float amplitude = 1 / (1 - Mathf.Exp(-gasTankParameter));
        float strength = gasForce * amplitude * (1 - Mathf.Exp(-gasTankParameter * tankRatio));
        rb.AddForce(gasVector * 2 * strength);
        currentGas -= 2 * Time.deltaTime;
        if (currentGas < 0) currentGas = 0;

    }
}
