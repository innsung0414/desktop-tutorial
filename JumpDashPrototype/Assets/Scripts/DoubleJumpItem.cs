using UnityEngine;

public class DoubleJumpItem : MonoBehaviour
{
    public float rotateSpeed = 48f;

    public AudioClip pickupSound;

    private SpriteRenderer itemRenderer;
    private Transform visibilityMarker;
    private Transform glowTransform;
    private Transform warmOutlineTransform;
    private Transform darkSilhouetteTransform;
    private Material visibilityMaterial;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool collected;

    private static readonly Color IceGlow = new Color(0.18f, 0.91f, 1f, 0.95f);
    private static readonly Color SignalGold = new Color(1f, 0.91f, 0.28f, 1f);
    private static readonly Color DeepCobalt = new Color(0.025f, 0.08f, 0.2f, 1f);

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale * 1.4f;
        originalPosition = transform.position;

        itemRenderer = GetComponent<SpriteRenderer>();
        if (itemRenderer == null || itemRenderer.sprite == null)
            return;

        // Preserve the ice artwork, but give it both a dark keyline and a warm signal edge.
        itemRenderer.color = Color.white;
        itemRenderer.sortingOrder = Mathf.Max(itemRenderer.sortingOrder, 24);

        BuildVisibilityMarker();

        GameObject glow = new GameObject("PickupVisibilityGlow");
        glow.transform.SetParent(transform, false);
        glow.transform.localScale = Vector3.one * 1.18f;
        glowTransform = glow.transform;

        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = itemRenderer.sprite;
        glowRenderer.sharedMaterial = itemRenderer.sharedMaterial;
        glowRenderer.sortingLayerID = itemRenderer.sortingLayerID;
        glowRenderer.sortingOrder = itemRenderer.sortingOrder - 1;
        glowRenderer.maskInteraction = itemRenderer.maskInteraction;
        glowRenderer.color = IceGlow;
    }

    void Update()
    {
        if (collected)
            return;

        float time = Time.time;
        transform.position = originalPosition + Vector3.up * (Mathf.Sin(time * 2.7f) * 0.1f);
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (visibilityMarker != null)
        {
            visibilityMarker.rotation = Quaternion.identity;
            float pulse = 1f + Mathf.Sin(time * 3.8f) * 0.075f;
            visibilityMarker.localScale = Vector3.one * pulse;
        }

        if (glowTransform != null)
        {
            float pulse = 1f + Mathf.Sin(time * 4.2f) * 0.1f;
            glowTransform.localScale = Vector3.one * (1.18f * pulse);
        }

        if (warmOutlineTransform != null)
        {
            float pulse = 1f + Mathf.Sin(time * 3.2f) * 0.035f;
            warmOutlineTransform.localScale = Vector3.one * (1.3f * pulse);
        }

        if (darkSilhouetteTransform != null)
        {
            float pulse = 1f + Mathf.Sin(time * 3.2f) * 0.035f;
            darkSilhouetteTransform.localScale = Vector3.one * (1.52f * pulse);
        }

    }

    private void BuildVisibilityMarker()
    {
        GameObject markerObject = new GameObject("DoubleJumpVisibilityMarker");
        markerObject.transform.SetParent(transform, false);
        visibilityMarker = markerObject.transform;

        int layer = itemRenderer.sortingLayerID;
        int order = itemRenderer.sortingOrder;
        visibilityMaterial = new Material(Shader.Find("Sprites/Default"));
        visibilityMaterial.hideFlags = HideFlags.DontSave;

        CreateBeam(visibilityMarker, visibilityMaterial, layer, order - 7);
        CreateRing("SignalGlow", visibilityMarker, visibilityMaterial, layer,
            order - 6, 0.96f, 0.28f, IceGlow);
        CreateRing("CobaltKeyline", visibilityMarker, visibilityMaterial, layer,
            order - 5, 0.96f, 0.16f, DeepCobalt);
        CreateRing("GoldSignal", visibilityMarker, visibilityMaterial, layer,
            order - 4, 0.96f, 0.075f, SignalGold);

        darkSilhouetteTransform = CreateIconLayer(
            "DarkIconKeyline", DeepCobalt, 1.52f, order - 3);
        warmOutlineTransform = CreateIconLayer(
            "GoldIconSignal", SignalGold, 1.3f, order - 2);
    }

    private Transform CreateIconLayer(string objectName, Color tint, float scale, int sortingOrder)
    {
        GameObject iconLayer = new GameObject(objectName);
        iconLayer.transform.SetParent(visibilityMarker, false);
        iconLayer.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = iconLayer.AddComponent<SpriteRenderer>();
        renderer.sprite = itemRenderer.sprite;
        renderer.sharedMaterial = itemRenderer.sharedMaterial;
        renderer.sortingLayerID = itemRenderer.sortingLayerID;
        renderer.sortingOrder = sortingOrder;
        renderer.maskInteraction = itemRenderer.maskInteraction;
        renderer.color = tint;
        return iconLayer.transform;
    }

    private static LineRenderer CreateRing(
        string objectName,
        Transform parent,
        Material material,
        int sortingLayer,
        int sortingOrder,
        float radius,
        float width,
        Color color)
    {
        GameObject ringObject = new GameObject(objectName);
        ringObject.transform.SetParent(parent, false);

        LineRenderer ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 48;
        ring.sharedMaterial = material;
        ring.startWidth = width;
        ring.endWidth = width;
        ring.startColor = color;
        ring.endColor = color;
        ring.numCornerVertices = 4;
        ring.numCapVertices = 4;
        ring.sortingLayerID = sortingLayer;
        ring.sortingOrder = sortingOrder;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;

        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }

        return ring;
    }

    private static LineRenderer CreateBeam(Transform parent, Material material, int sortingLayer, int sortingOrder)
    {
        GameObject beamObject = new GameObject("VerticalPickupBeacon");
        beamObject.transform.SetParent(parent, false);

        LineRenderer beam = beamObject.AddComponent<LineRenderer>();
        beam.useWorldSpace = false;
        beam.positionCount = 3;
        beam.SetPosition(0, new Vector3(0f, -1.65f, 0f));
        beam.SetPosition(1, Vector3.zero);
        beam.SetPosition(2, new Vector3(0f, 1.65f, 0f));
        beam.sharedMaterial = material;
        beam.widthMultiplier = 0.42f;
        beam.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.5f, 0.38f), new Keyframe(1f, 0f));
        Gradient beamGradient = new Gradient();
        beamGradient.SetKeys(
            new[]
            {
                new GradientColorKey(IceGlow, 0f),
                new GradientColorKey(SignalGold, 0.5f),
                new GradientColorKey(IceGlow, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.38f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        beam.colorGradient = beamGradient;
        beam.sortingLayerID = sortingLayer;
        beam.sortingOrder = sortingOrder;
        beam.numCapVertices = 4;
        beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beam.receiveShadows = false;
        return beam;
    }

    private void OnDestroy()
    {
        if (visibilityMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(visibilityMaterial);
            else
                DestroyImmediate(visibilityMaterial);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected)
            return;

        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (player == null)
            return;

        collected = true;
        player.AddDoubleJumpItem();

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Sprite icon = itemRenderer != null ? itemRenderer.sprite : null;
        DoubleJumpPickupBurst.Play(icon, transform.position, itemRenderer);
        Destroy(gameObject);
    }
}
