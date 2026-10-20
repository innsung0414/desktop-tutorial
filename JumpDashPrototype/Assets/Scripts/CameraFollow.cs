using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float offsetX = 5f;

    void Update()
    {
        transform.position = new Vector3(
            player.position.x + offsetX,
            transform.position.y,
            transform.position.z
        );
    }
}