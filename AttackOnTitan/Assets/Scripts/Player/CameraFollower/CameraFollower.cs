using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    [SerializeField]
    Transform VRCamera;

    [SerializeField]
    float hipRatio = 2f;

    float yOffset = 0;

    float yRot = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        yOffset = VRCamera.transform.localPosition.y / hipRatio;
        transform.position = new Vector3(VRCamera.position.x, VRCamera.parent.position.y + yOffset, VRCamera.position.z);

        //Rotation following with quaternions
        yRot = Quaternion.ToEulerAngles(VRCamera.rotation).y * 180 / Mathf.PI;
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}
