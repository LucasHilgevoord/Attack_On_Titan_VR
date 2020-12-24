using UnityEngine;

public class PelvisTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform playerHead;

    [Header("Parameters")]
    [SerializeField] float neckRadius = 0.2f;
    [SerializeField] float heightRatio = 0.5f;

    private Vector3 forward;
    private Vector3 pForward;
    private Vector3 pUp;

    // Update is called once per frame
    void Update()
    {
        //Position
        transform.localPosition = new Vector3(playerHead.localPosition.x - player.InverseTransformDirection(playerHead.up).x * neckRadius,
            playerHead.localPosition.y * heightRatio,
            playerHead.localPosition.z - player.InverseTransformDirection(playerHead.up).z * neckRadius);

        //Rotation
        pForward = player.InverseTransformDirection(playerHead.forward);
        pUp = player.InverseTransformDirection(playerHead.up);
        forward = pForward;
        forward.x += pUp.x * -pForward.y;
        forward.z += pUp.z * -pForward.y;
        forward.y = 0;
        transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
