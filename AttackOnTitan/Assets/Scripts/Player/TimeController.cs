using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] public float originalMass;

    [SerializeField] private float _timeSpeed = 1;

    [SerializeField] public bool enableBubble;

    [SerializeField] public SphereCollider bubble;

    //List<AffectedObject> affectedObjects;
    List<Rigidbody> affectedRB = new List<Rigidbody>();

    private float prevTimeSpeed = 1;

    public float TimeSpeed
    {
        get
        {
            return _timeSpeed;
        }
        set
        {
            _timeSpeed = value;
            UpdateTimeSpeed();
        }
    }

    private void Start()
    {
        prevTimeSpeed = _timeSpeed;
    }

    private void FixedUpdate()
    {
        if (rb.useGravity)
        {
            rb.velocity += Physics.gravity * Time.deltaTime * -1; //Cancel standard gravity
            rb.AddForce(Physics.gravity * originalMass); //Add gravity force
        }

        if (enableBubble)
        {
            foreach (Rigidbody body in affectedRB)
            {
                if (body.useGravity)
                {
                    body.velocity += Physics.gravity * Time.deltaTime * -1;
                    body.AddForce(Physics.gravity * rb.mass * Mathf.Pow(TimeSpeed, 2));
                }
            }
        }
    }

    private void OnValidate()
    {
        UpdateTimeSpeed();
    }

    private void UpdateTimeSpeed()
    {
        if (_timeSpeed <= 0) _timeSpeed = 1e-10f;
        rb.mass = originalMass / Mathf.Pow(_timeSpeed, 2);
        rb.velocity *= _timeSpeed / prevTimeSpeed;
        rb.angularVelocity *= _timeSpeed / prevTimeSpeed;
        prevTimeSpeed = _timeSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body;
        if(other.gameObject.TryGetComponent<Rigidbody>(out body))
        {
            affectedRB.Add(body);
            body.mass /= Mathf.Pow(TimeSpeed, 2);
            body.velocity *= TimeSpeed;
            body.angularVelocity *= TimeSpeed;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody body;
        if (other.gameObject.TryGetComponent<Rigidbody>(out body))
        {
            affectedRB.Remove(body);
            body.mass *= Mathf.Pow(TimeSpeed, 2);
            body.velocity /= TimeSpeed;
            body.angularVelocity /= TimeSpeed;
        }
    }
}