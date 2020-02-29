using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchBehavior : MonoBehaviour
{
    public StateMachine controller;
    private GameObject target;
    private GameObject[] players;
    private List<GameObject> playersInRange;

    private float objectHeight;
    public LayerMask mask;
    private LayerMask ignoreMask;

    private float checkDistanceDelay = 1; //Delay until it checks where all players are.
    private float timer;

    private bool useSenses = false;
    private float sightRange = 10;
    private float sightAngle = 45;
    private float personalSpaceRange = 1; // Range around entity where it still knows the targets location.

    private float forgetTimer = 0; // How long it takes until the entity forgets the target
    private float currentForgetTime;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        playersInRange = new List<GameObject>();

        objectHeight = GetComponent<Collider>().bounds.max.y;
        ignoreMask = ~mask.value;

        timer = checkDistanceDelay;
        currentForgetTime = forgetTimer;

        //TESTING
        target = players[0];
        controller.ChangeState("CHASE", new object[] { (object)target });
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            CheckPlayerDistance();
        }

        if (!useSenses) { return; }
        //SpotPlayer();
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
            Vector3 origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            //Check with raycast if the player is within the sight angle
            if (angleToPlayer >= -sightAngle && angleToPlayer <= sightAngle)
            {
                RaycastHit hit;
                if (Physics.Raycast(origin, targetDir, out hit, sightRange))
                {
                    Debug.DrawLine(origin, hit.collider.transform.position, Color.red);
                    //Debug.Log(hit.collider.name);
                    if (hit.collider.CompareTag("Player"))
                    {
                        if (target)
                        {
                            Debug.DrawLine(origin, target.transform.position, Color.blue);
                        }
                        if (target != playersInRange[i] && target == null)
                        {
                            target = playersInRange[i];
                            controller.ChangeState("CHASE", new object[] { (object)target });
                        }
                        //Reset timer if player is back in sight after it disappeared.
                        if (target == playersInRange[i] && currentForgetTime != forgetTimer)
                        {
                            Debug.Log("RESETTING----------------");
                            currentForgetTime = forgetTimer;
                        }
                    }
                    //When the spotted player is behind something
                    else
                    {
                        Debug.Log(hit.collider.name);

                        if (target == playersInRange[i])
                        {
                            Debug.Log("Behind something");
                            ForgetPlayer();
                        }
                    }
                }
            }
            else if (target == playersInRange[i] && (transform.position - players[i].transform.position).magnitude < personalSpaceRange)
            {
                return;
            }
            //When spotted is out of the sight angle
            else if (target == playersInRange[i])
            {
                Debug.Log("Out of Sight angle");
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
        //Debug.Log(currentForgetTime);
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
