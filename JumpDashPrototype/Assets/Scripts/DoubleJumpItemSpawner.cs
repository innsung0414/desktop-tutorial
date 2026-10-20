using UnityEngine;

public class DoubleJumpItemSpawner : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform player;
    public float spawnDistanceAhead = 25f;
    public float minSpawnTime = 8f;
    public float maxSpawnTime = 15f;

    private System.Collections.IEnumerator Start()
    {
        while (TitleManager.IsAttractMode)
            yield return null;

        SpawnItem();
    }

    private void SpawnItem()
    {
        if (player == null || itemPrefab == null)
            return;

        float x = player.position.x + spawnDistanceAhead;
        float groundY = ResolveGroundSurfaceY();
        // Put the collectible within the normal jump arc, rather than several units
        // above the rider where it cannot be reached.
        float y = groundY + Random.Range(1.1f, 1.55f);
        Instantiate(itemPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        Invoke(nameof(SpawnItem), Random.Range(minSpawnTime, maxSpawnTime));
    }

    private float ResolveGroundSurfaceY()
    {
        GameObject ground = GameObject.Find("GroundCollisionSurface");
        CompositeCollider2D surface = ground != null ? ground.GetComponent<CompositeCollider2D>() : null;
        if (surface != null)
        {
            Physics2D.SyncTransforms();
            return surface.bounds.max.y;
        }

        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        return playerCollider != null ? playerCollider.bounds.min.y : player.position.y - 0.25f;
    }
}
