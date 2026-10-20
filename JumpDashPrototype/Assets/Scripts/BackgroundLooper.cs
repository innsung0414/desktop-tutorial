using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    public Transform player;

    public float backgroundWidth = 35f;

    void Update()
    {
        if (player.position.x > transform.position.x + backgroundWidth)
        {
            transform.position += new Vector3(
                backgroundWidth * 3f,
                0f,
                0f
            );
        }
    }
}
