using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform playerHead;
    [SerializeField] CapsuleCollider col;

    [Header("Parameters")]
    [SerializeField] float neckRadius = 0.2f;
    

    // Update is called once per frame
    void Update()
    {
        col.center = new Vector3(playerHead.localPosition.x -player.InverseTransformDirection(playerHead.up).x * neckRadius,
            playerHead.localPosition.y / 2,
            playerHead.localPosition.z - player.InverseTransformDirection(playerHead.up).z * neckRadius);
        col.height = playerHead.localPosition.y;
    }
}
