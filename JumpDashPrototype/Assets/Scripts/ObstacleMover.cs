using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float destroyDistanceBehindPlayer = 15f;

    private Transform player;

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (player != null && transform.position.x < player.position.x - destroyDistanceBehindPlayer)
        {
            Destroy(gameObject);
        }
    }
}