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
    [SerializeField] private ParticleSystem leftGas;
    [SerializeField] private ParticleSystem rightGas;

    [Header("(Optional) Time Controller")]
    [Tooltip("If a Time Controller is applied to the rigid body, add it here so the calculations will still be valid.")]
    [SerializeField] private TimeController timeController;

    [Header("Stats")]
    [Tooltip("The force of each gas thruster. Using both hands will result in two separate forces with this magnitude.")]
    [SerializeField] private float gasForce = 7;
    [Tooltip("Gas burst gives an instant velocity change of this magnitude.")]
    [SerializeField] private float gasBurstSpeed = 3.5f;
    [Tooltip("How much gas the gas burst uses.")]
    [SerializeField] private float gasBurstCost = 5;
    [Tooltip("Performance of gas tank goes down when gas is almost empty. The higher Gas Tank Parameter is, the more 'square' the graph looks of force vs tank content.")]
    [SerializeField] private float gasTankParameter = 10;

    [Header("Gas tank")]
    [Tooltip("The volume of the gas tank in seconds of use. Using both hands means gas will deplete 2x faster.")]
    [SerializeField] private float maxGas = 10000;
    [Tooltip("Current amount of gas (see Max Gas).")]
    [SerializeField] private float currentGas = 10000;

    [Header("Options")]
    [Tooltip("False: Gas comes from palms.\nTrue: Kinda like a jetpack, gas is controlled with left trackpad.")]
    [SerializeField] private bool altGasMode = false;
    [Tooltip("If gas is activated 2 times within this time apart, gas burst activates (seperate for both hands).")]
    [SerializeField] private float burstActivationTime = 0.5f;

    [Header("Calculation vars")]
    private bool activateGasLeft = false;
    private bool activateGasRight = false;
    private bool activateGasBurstLeft = false;
    private bool activateGasBurstRight = false;
    private float[] burstTimer = { 0, 0 };
    private float burstCooldown = 0.5f;
    private float[] burstCooldownTimer = { 0, 0 };

    #endregion

    // Update is called once per frame
    private void Update()
    {
        if (SteamVR_Actions.player_controller.grip_left.state || SteamVR_Actions.player_controller_gripForce_left.axis > 0.3f)
        {
            activateGasLeft = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasLeft = false;
        }

        if (SteamVR_Actions.player_controller.grip_right.state || SteamVR_Actions.player_controller_gripForce_right.axis > 0.3f)
        {
            activateGasRight = true;
            //Instanciate smoke on gaspoint.pos
        }
        else
        {
            activateGasRight = false;
        }

        //Manage gas burst
        if (burstCooldownTimer[0] < 0) //left
        {
            if (SteamVR_Actions.player_controller.grip_left.GetStateDown(SteamVR_Input_Sources.Any)
            || (SteamVR_Actions.player_controller.gripForce_left.axis > 0.3f && SteamVR_Actions.player_controller.gripForce_left.delta > 0.3f))
            {
                if (burstTimer[0] > 0)
                {
                    activateGasBurstLeft = true;
                    burstCooldownTimer[0] = burstCooldown;
                    burstTimer[0] = 0;
                }
                else burstTimer[0] = burstActivationTime;
            }
            burstTimer[0] -= Time.deltaTime;
        }
        else
            burstCooldownTimer[0] -= Time.deltaTime;

        if (burstCooldownTimer[1] < 0)
        {
            if (SteamVR_Actions.player_controller.grip_right.GetStateDown(SteamVR_Input_Sources.Any)
            || (SteamVR_Actions.player_controller.gripForce_right.axis > 0.3f && SteamVR_Actions.player_controller.gripForce_right.delta > 0.3f))
            {
                if (burstTimer[1] > 0)
                {
                    activateGasBurstRight = true;
                    burstCooldownTimer[1] = burstCooldown;
                    burstTimer[1] = 0;
                }
                else burstTimer[1] = burstActivationTime;
            }
            burstTimer[1] -= Time.deltaTime;
        }
        else
            burstCooldownTimer[1] -= Time.deltaTime;
        
    }

    private void FixedUpdate()
    {
        leftGas.enableEmission = false;
        rightGas.enableEmission = false;
        if (altGasMode)
        {
            if (activateGasLeft || activateGasRight) altUseGas();
        }
        else
        {
            if (activateGasLeft) useGas(leftHandTarget, leftGas, false, 0); //Gas left side
            if (activateGasRight) useGas(rightHandTarget, rightGas, false, 1); //Gas right side
            if (activateGasBurstLeft)
            {
                useGas(leftHandTarget, leftGas, true, 0); //Gas burst left
                activateGasBurstLeft = false;
            }
            if (activateGasBurstRight)
            {
                useGas(rightHandTarget, rightGas, true, 1); //Gas burst right
                activateGasBurstRight = false;
            }
        }
    }

    private void useGas(Transform hand, ParticleSystem gas, bool burst, int flip)
    {
        //Code to weaken force when tank is almost empty
        float tankRatio = currentGas / maxGas;
        float amplitude = 1 / (1 - Mathf.Exp(-gasTankParameter));
        float strength = amplitude * (1 - Mathf.Exp(-gasTankParameter * tankRatio));

        if (burst)
        {
            //rb.AddForce(hand.right * Mathf.Pow(-1, flip) * gasBurstSpeed * strength, ForceMode.Impulse); //same as below, but add a specific speed instantly
            TimeFunctions.AddRealVelocity(hand.right * Mathf.Pow(-1, flip) * gasBurstSpeed * strength, timeController, rb);
            currentGas -= gasBurstCost;
            if (currentGas < 0) currentGas = 0;
        }
        else
        {
            rb.AddForce(hand.right * Mathf.Pow(-1, flip) * strength * gasForce); //Note: side=0 is left, side=1 is right
            //Also note: For the right hand (side=1) I need the right vector flipped, that's where the power of -1 is for.
            currentGas -= TimeFunctions.DeltaTime(timeController);
            if (currentGas < 0) currentGas = 0;
            gas.enableEmission = true;
        }
    }

    private void altUseGas() //Warning: This is not up to date yet
    {
        //Note: It kinda works but is not stable yet
        playerController.UpdateWalkingVector();
        float vectorHeight = Mathf.Sqrt(1 - playerController.walkingVector3.sqrMagnitude);
        Vector3 gasVector = playerController.walkingVector3 + new Vector3(0, vectorHeight, 0);

        float tankRatio = currentGas / maxGas;
        float amplitude = 1 / (1 - Mathf.Exp(-gasTankParameter));
        float strength = gasForce * amplitude * (1 - Mathf.Exp(-gasTankParameter * tankRatio));
        rb.AddForce(gasVector * 2 * strength);
        currentGas -= 2 * TimeFunctions.DeltaTime(timeController);
        if (currentGas < 0) currentGas = 0;

    }
}
