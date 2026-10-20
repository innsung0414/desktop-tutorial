using UnityEngine;

public class GroundLooper : MonoBehaviour
{
    public Transform player;
    public float groundWidth = 20f;
    public int totalGroundCount = 3;

    void Update()
    {
        if (player == null)
            return;

        if (player.position.x > transform.position.x + groundWidth)
        {
            transform.position += new Vector3(groundWidth * totalGroundCount, 0f, 0f);

            // Keep the physical surface continuous when a visual segment wraps.
            CompositeCollider2D surface = GetComponentInParent<CompositeCollider2D>();
            if (surface != null)
            {
                Physics2D.SyncTransforms();
                surface.GenerateGeometry();
            }
        }
    }
}
