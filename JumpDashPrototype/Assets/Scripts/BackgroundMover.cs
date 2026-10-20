using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    public float moveSpeed = 2f;

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < -30f)
        {
            transform.position = new Vector3(30f, transform.position.y, transform.position.z);
        }
    }
}