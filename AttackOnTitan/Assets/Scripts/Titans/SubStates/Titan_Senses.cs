using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan_Senses : MonoBehaviour
{

    private GameObject[] players;
    private List<GameObject> playersInRange;
    private float checkDistanceDelay = 1;

    private bool useSenses = false;
    private GameObject target;
    private float sightRange = 10;
    private float sightAngle = 45;

    private float forgetTimer = 5;
    private float currentForgetTime;

    // Start is called before the first frame update
    void Start()
    {
        //Replace with reference--------
        players = GameObject.FindGameObjectsWithTag("Player");
        playersInRange = new List<GameObject>();

        currentForgetTime = forgetTimer;

        if (players.Length == 0) { return; }
        InvokeRepeating("CheckPlayerDistance", 0, checkDistanceDelay);
    }

    // Update is called once per frame
    void Update()
    {
        if (!useSenses) { Debug.Log("System: Sensing = DISABLED");  return; }
        Debug.Log("System: Sensing = ENABLED");
        SpotPlayer();
        if (target != null) { Debug.DrawLine(transform.position, new Vector3(target.transform.position.x, target.transform.position.y + 0.1f, target.transform.position.z), Color.red); }
        for (int i = 0; i < playersInRange.Count; i++)
        {
            Debug.DrawLine(transform.position, new Vector3(playersInRange[i].transform.position.x, playersInRange[i].transform.position.y - 0.1f, playersInRange[i].transform.position.z), Color.blue);
        }
    }

    /// <summary>
    /// Sending raycasts to players within range to see if the titan sees someone.
    /// </summary>
    private void SpotPlayer()
    {
        for (int i = 0; i < playersInRange.Count; i++)
        {
            Vector3 targetDir = playersInRange[i].transform.position - transform.position;
            float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));
            Vector3 origin = transform.position;
            Debug.DrawLine(origin, origin + transform.forward * sightRange, Color.green);

            //Check with raycast if player is seen when the player is within the sight angle
            if (angleToPlayer >= -sightAngle && angleToPlayer <= sightAngle)
            {
                RaycastHit hit;
                if (Physics.Raycast(origin, targetDir, out hit, sightRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawLine(origin, hit.point, Color.cyan);
                        if (target != playersInRange[i] && target == null)
                        {
                            target = playersInRange[i];
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
                        Debug.DrawLine(origin, hit.point, Color.yellow);
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
        if (currentForgetTime <= 0)
        {
            target = null;
            currentForgetTime = forgetTimer;
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
                    //Debug.Log("Added: " + players[i].transform.name);
                }
            }
            else if (playersInRange.Contains(players[i]))
            {
                playersInRange.Remove(players[i]);
                //Debug.Log("Removed: " + players[i].transform.name);
            }
        }
        useSenses = (playersInRange.Count != 0) ? true : false;
    }
}
