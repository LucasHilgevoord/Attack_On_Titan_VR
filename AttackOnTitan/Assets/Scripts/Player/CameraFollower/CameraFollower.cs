using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    [SerializeField]
    Transform VRCamera;

    [SerializeField]
    float hipRatio = 2f;

    [SerializeField]
    float rotationRatio = 0.2f;

    [SerializeField]
    float forwardOffset = 0.2f;

    [SerializeField]
    float neckRadius = 0.1f;

    float xOffset = 0f;
    float yOffset = 0f;
    float zOffset = 0f;

    Vector3 forward;
    Vector3 horizontal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Set the body at a certain ratio of your current height, so it will always be somewhere around your actual hips
        yOffset = VRCamera.transform.localPosition.y / hipRatio;

        //Set xOffset and zOffset according to sideways and forwards/backwards head rotation
        xOffset = -VRCamera.up.x * neckRadius;
        zOffset = -VRCamera.up.z * neckRadius;
        transform.position = new Vector3(VRCamera.position.x + xOffset, VRCamera.parent.position.y + yOffset, VRCamera.position.z + zOffset);

        //Complex but nice rotation. Body rotates with you on Y-axis and partially rotates vertically. It won't break when looking completely up or downwards.
        forward = VRCamera.forward;
        forward.x += VRCamera.up.x * -VRCamera.forward.y;
        forward.z += VRCamera.up.z * -VRCamera.forward.y;
        horizontal = forward;
        horizontal.y = 0f;
        forward.y = forward.y * rotationRatio * Vector3.Magnitude(horizontal);
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        //Now set the body a bit forward so you can actually see it
        transform.position += transform.forward * forwardOffset;
    }
}
