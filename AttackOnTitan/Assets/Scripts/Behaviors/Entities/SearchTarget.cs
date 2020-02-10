using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchTarget : MonoBehaviour
{
    private GameObject target;
    public StateMachine controller;
    private GameObject[] players;
    private List<GameObject> playersInRange;
    private float checkDistanceDelay = 1;

    private bool useSenses = false;
    private float sightRange = 10;
    private float sightAngle = 45;

    private float forgetTimer = 5;
    private float currentForgetTime;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        playersInRange = new List<GameObject>();

        currentForgetTime = forgetTimer;

        if (players.Length == 0) { return; }
        InvokeRepeating("CheckPlayerDistance", 0, checkDistanceDelay);
    }

    // Update is called once per frame
    void Update()
    {
        if (!useSenses) { return; }
        SpotPlayer();
    }

    /// <summary>
    /// Sending raycasts to players within range to see if the titan sees someone.
    /// </summary>
    private void SpotPlayer()
    {
        for (int i = 0; i < playersInRange.Count; i++)
        {
            //Calculating player[i]'s position/angle
            Vector3 targetDir = playersInRange[i].transform.position - transform.position;
            float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
            Vector3 origin = transform.position;

            //Check with raycast if player is seen when the player is within the sight angle
            if (angleToPlayer >= -sightAngle && angleToPlayer <= sightAngle)
            {
                RaycastHit hit;
                if (Physics.Raycast(origin, targetDir, out hit, sightRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        if (target != playersInRange[i] && target == null)
                        {
                            target = playersInRange[i];
                            controller.ChangeState("CHASE", new object[] { (object)target });
                            //Change behavior state to chase
                        }
                        //Reset timer if player is back in sight after disappear.
                        if (target == playersInRange[i] && currentForgetTime != forgetTimer)
                        {
                            currentForgetTime = forgetTimer;
                        }
                    }
                    //When player[i] is behind something
                    else
                    {
                        if (target == playersInRange[i])
                        {
                            ForgetPlayer();
                        }
                    }
                }
            }
            //When player is out of the sight angle 
            else if (target == playersInRange[i]) 
            {
                ForgetPlayer();
            }
        }
    }

    /// <summary>
    /// Resetting target when titan has lost the target.
    /// </summary>
    private void ForgetPlayer()
    {
        currentForgetTime -= Time.deltaTime;
        Debug.Log((int)currentForgetTime);
        if (currentForgetTime <= 0)
        {
            currentForgetTime = forgetTimer;
            controller.ChangeState("SEARCH", new object[] { (object)target });
            target = null;
        }
    }

    /// <summary>
    /// Check if players are close enough before activating the senses.
    /// </summary>
    private void CheckPlayerDistance()
    {
        for (int i = 0; i < players.Length; i++)
        {
            float distance = ((transform.position - players[i].transform.position).magnitude);
            //Check if the player in index is close enough to look for it and add it to inRange list.
            if (distance < sightRange)
            {
                if (!playersInRange.Contains(players[i]))
                {
                    playersInRange.Add(players[i]);
                }
            }
            else if (playersInRange.Contains(players[i]))
            {
                if (target == players[i])
                    target = null;

                playersInRange.Remove(players[i]);
            }
        }
        useSenses = (playersInRange.Count != 0) ? true : false;
    }
}
