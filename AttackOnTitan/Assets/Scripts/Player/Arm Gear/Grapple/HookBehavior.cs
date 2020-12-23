using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookBehavior : MonoBehaviour
{

    public bool collided = false;
    public GameObject collidedObject;
    private GameObject[] players;
    private GameObject[] grapples;

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < players.Length; i++)
        {
            Physics.IgnoreCollision(players[i].gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }

        grapples = GameObject.FindGameObjectsWithTag("Grapple");
        for (int i = 0; i < grapples.Length; i++)
        {
            Physics.IgnoreCollision(grapples[i].gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }
    }

    private void Update()
    {
        //if(!collided) transform.Rotate(new Vector3(0, 0, 1), grappleRotSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        if(!collided)
        {
            collidedObject = other.gameObject;
            collided = true;
            GetComponent<Collider>().enabled = false;
        }
    }
    void OnCollisionExit(Collision other)
    {
        collided = false;
    }
}
