using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs (Small, Tall, Wide)")]
    public GameObject[] obstaclePrefabs;

    [Header("Player Reference")]
    public Transform player;

    [Header("Spawn Position")]
    public float spawnDistanceAhead = 20f;
    public float groundY = -2f;

    [Header("Transparent Sprite Ground Compensation")]
    [Tooltip("Small's artwork has transparent pixels beneath its visible edge; overlap it slightly into the snow.")]
    public float smallVisualGroundInset = 0.17f;
    [Tooltip("Wide's artwork has transparent pixels beneath its visible edge; overlap it slightly into the snow.")]
    public float wideVisualGroundInset = 0.17f;

    [Header("Spawn Timing")]
    public float minSpawnDelay = 2.4f;
    public float maxSpawnDelay = 3.2f;

    [Header("Difficulty Scaling")]
    public float spawnAcceleration = 0.025f;
    public float minimumSpawnDelayFloor = 1.35f;
    public float maximumSpawnDelayFloor = 1.9f;
    public float minimumPatternGap = 11f;

    private float startingMinSpawnDelay;
    private float startingMaxSpawnDelay;
    private float spawnStartTime;
    private float furthestSpawnedObstacleX;
    private bool hasSpawnedPattern;
    private bool warnedMissingReferences;
    private float cachedGroundSurfaceY;
    private bool hasGroundSurfaceY;

    private System.Collections.IEnumerator Start()
    {
        while (TitleManager.IsAttractMode)
            yield return null;

        startingMinSpawnDelay = minSpawnDelay;
        startingMaxSpawnDelay = maxSpawnDelay;
        spawnStartTime = Time.timeSinceLevelLoad;
        Invoke(nameof(SpawnPattern), 2f);
    }

    private void SpawnPattern()
    {
        if (player == null || obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            if (!warnedMissingReferences)
            {
                Debug.LogWarning("ObstacleSpawner needs a player and at least one obstacle prefab.", this);
                warnedMissingReferences = true;
            }
            return;
        }

        float spawnX = player.position.x + spawnDistanceAhead;
        if (hasSpawnedPattern)
            spawnX = Mathf.Max(spawnX, furthestSpawnedObstacleX + minimumPatternGap);
        float elapsedTime = Time.timeSinceLevelLoad - spawnStartTime;
        float difficultyReduction = Mathf.Max(0f, spawnAcceleration) * elapsedTime;
        minSpawnDelay = Mathf.Max(minimumSpawnDelayFloor, startingMinSpawnDelay - difficultyReduction);
        maxSpawnDelay = Mathf.Max(maximumSpawnDelayFloor, startingMaxSpawnDelay - difficultyReduction);
        maxSpawnDelay = Mathf.Max(maxSpawnDelay, minSpawnDelay);

        // Keep single hazards and simple pairs available longer before introducing denser combinations.
        int availablePatterns = elapsedTime < 18f ? 6 : elapsedTime < 42f ? 11 : 16;
        int pattern = Random.Range(0, availablePatterns);
        SpawnPatternByIndex(pattern, spawnX);

        Invoke(nameof(SpawnPattern), Random.Range(minSpawnDelay, maxSpawnDelay));
    }

    private void SpawnPatternByIndex(int pattern, float x)
    {
        switch (pattern)
        {
            case 0: SpawnAt(0, x); break; // Single small mogul
            case 1: SpawnAt(1, x); break; // Single tall ice pine
            case 2: SpawnAt(2, x); break; // Single wide glacier shelf
            case 3: SpawnAt(0, x); SpawnAt(0, x + 11.5f); break; // Relaxed two-mogul rhythm
            case 4: SpawnAt(0, x); SpawnAt(1, x + 11.5f); break; // Small then tall
            case 5: SpawnAt(1, x); SpawnAt(0, x + 11.5f); break; // Tall then small
            case 6: SpawnAt(0, x); SpawnAt(0, x + 11.5f); SpawnAt(0, x + 23f); break; // Three-mogul rhythm
            case 7: SpawnAt(1, x); SpawnAt(0, x + 11f); SpawnAt(1, x + 22f); break; // Tall slalom
            case 8: SpawnAt(2, x); SpawnAt(0, x + 11.5f); break; // Shelf then mogul
            case 9: SpawnAt(0, x); SpawnAt(2, x + 11.5f); break; // Mogul then shelf
            case 10: SpawnAt(2, x); SpawnAt(2, x + 12f); break; // Double shelf
            case 11: SpawnAt(0, x); SpawnAt(1, x + 11.5f); SpawnAt(0, x + 23f); break; // Low-high-low
            case 12: SpawnAt(1, x); SpawnAt(1, x + 11.5f); break; // Twin ice pines
            case 13: SpawnAt(2, x); SpawnAt(1, x + 11.5f); break; // Wide-to-tall check
            case 14: SpawnAt(0, x); SpawnAt(2, x + 11.5f); SpawnAt(1, x + 23f); break; // Small-shelf-tall
            default: SpawnAt(1, x); SpawnAt(2, x + 11.5f); SpawnAt(0, x + 23f); break; // Tall-shelf-small slalom
        }
    }

    private void SpawnAt(int prefabIndex, float x)
    {
        GameObject prefab = GetObstaclePrefab(prefabIndex);
        if (prefab == null)
            return;

        furthestSpawnedObstacleX = hasSpawnedPattern
            ? Mathf.Max(furthestSpawnedObstacleX, x)
            : x;
        hasSpawnedPattern = true;
        GameObject obstacle = Instantiate(prefab, new Vector3(x, groundY, 0f), Quaternion.identity);
        PlaceObstacleOnGround(obstacle, prefabIndex);
    }

    private void PlaceObstacleOnGround(GameObject obstacle, int prefabIndex)
    {
        Collider2D obstacleCollider = obstacle.GetComponent<Collider2D>();
        if (obstacleCollider == null)
            obstacleCollider = obstacle.GetComponentInChildren<Collider2D>();
        if (obstacleCollider == null)
            return;

        float surfaceY = ResolveGroundSurfaceY();
        Physics2D.SyncTransforms();
        Vector3 position = obstacle.transform.position;
        position.y += surfaceY - obstacleCollider.bounds.min.y;
        obstacle.transform.position = position;
        Physics2D.SyncTransforms();

        float visualGroundInset = prefabIndex == 0
            ? smallVisualGroundInset
            : prefabIndex == 2 ? wideVisualGroundInset : 0f;
        AlignObstacleVisual(obstacle, obstacleCollider, visualGroundInset);
    }

    private float ResolveGroundSurfaceY()
    {
        if (hasGroundSurfaceY)
            return cachedGroundSurfaceY;

        GameObject ground = GameObject.Find("GroundCollisionSurface");
        CompositeCollider2D surface = ground != null ? ground.GetComponent<CompositeCollider2D>() : null;
        if (surface != null)
        {
            Physics2D.SyncTransforms();
            cachedGroundSurfaceY = surface.bounds.max.y;
        }
        else
        {
            // MainScene's ground is one unit deep and its current root spawn height is -2.
            cachedGroundSurfaceY = groundY - 0.5f;
        }

        hasGroundSurfaceY = true;
        return cachedGroundSurfaceY;
    }

    private static void AlignObstacleVisual(GameObject obstacle, Collider2D obstacleCollider, float visualGroundInset)
    {
        SpriteRenderer source = obstacle.GetComponent<SpriteRenderer>();
        if (source == null || source.sprite == null ||
            source.sprite.bounds.size.x <= 0f || source.sprite.bounds.size.y <= 0f)
            return;

        Transform art = obstacle.transform.Find("GroundAlignedVisual");
        if (art == null)
        {
            GameObject artObject = new GameObject("GroundAlignedVisual");
            art = artObject.transform;
            art.SetParent(obstacle.transform, false);
        }

        Vector3 parentScale = obstacle.transform.lossyScale;
        float parentScaleX = Mathf.Max(0.0001f, Mathf.Abs(parentScale.x));
        float parentScaleY = Mathf.Max(0.0001f, Mathf.Abs(parentScale.y));
        float visualScaleX = obstacleCollider.bounds.size.x / (source.sprite.bounds.size.x * parentScaleX);
        float visualScaleY = obstacleCollider.bounds.size.y / (source.sprite.bounds.size.y * parentScaleY);
        Vector3 colliderCenterLocal = obstacle.transform.InverseTransformPoint(obstacleCollider.bounds.center);
        Vector3 spriteOriginLocal = new Vector3(
            colliderCenterLocal.x - source.sprite.bounds.center.x * visualScaleX,
            colliderCenterLocal.y - source.sprite.bounds.center.y * visualScaleY - visualGroundInset / parentScaleY,
            0f);
        art.localPosition = spriteOriginLocal;
        art.localScale = new Vector3(visualScaleX, visualScaleY, 1f);

        SpriteRenderer alignedRenderer = art.GetComponent<SpriteRenderer>();
        if (alignedRenderer == null)
            alignedRenderer = art.gameObject.AddComponent<SpriteRenderer>();
        CopyRendererSettings(source, alignedRenderer);
        source.enabled = false;

        Transform outline = obstacle.transform.Find("VisibilityOutline");
        if (outline != null)
        {
            outline.SetParent(art, false);
            outline.localPosition = Vector3.zero;
            outline.localScale = new Vector3(1.14f, 1f, 1f);

            SpriteRenderer outlineRenderer = outline.GetComponent<SpriteRenderer>();
            if (outlineRenderer != null)
                outlineRenderer.sortingOrder = alignedRenderer.sortingOrder - 1;
        }
    }

    private static void CopyRendererSettings(SpriteRenderer source, SpriteRenderer destination)
    {
        destination.sprite = source.sprite;
        destination.color = source.color;
        destination.sharedMaterial = source.sharedMaterial;
        destination.sortingLayerID = source.sortingLayerID;
        destination.sortingOrder = source.sortingOrder;
        destination.maskInteraction = source.maskInteraction;
        destination.flipX = source.flipX;
        destination.flipY = source.flipY;
        destination.drawMode = source.drawMode;
        destination.size = source.size;
        destination.spriteSortPoint = source.spriteSortPoint;
        destination.renderingLayerMask = source.renderingLayerMask;
    }

    private GameObject GetObstaclePrefab(int index)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return null;

        if (index < obstaclePrefabs.Length && obstaclePrefabs[index] != null)
            return obstaclePrefabs[index];

        for (int i = 0; i < obstaclePrefabs.Length; i++)
        {
            if (obstaclePrefabs[i] != null)
                return obstaclePrefabs[i];
        }

        return null;
    }
}
