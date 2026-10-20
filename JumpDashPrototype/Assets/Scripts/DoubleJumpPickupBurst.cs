using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a self-contained ice-shard pickup burst without relying on the old
/// authored pickup prefab or its orange particle palette.
/// </summary>
public sealed class DoubleJumpPickupBurst : MonoBehaviour
{
    private enum MotionStyle
    {
        Expand,
        IconPop,
        Shard
    }

    private sealed class BurstElement
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public Color color;
        public Vector3 startPosition;
        public Vector2 direction;
        public float startScale;
        public float endScale;
        public float startDelay;
        public float duration;
        public float speed;
        public float gravity;
        public float startRotation;
        public float rotationSpeed;
        public MotionStyle style;
    }

    private readonly List<BurstElement> elements = new List<BurstElement>();
    private readonly List<Sprite> generatedSprites = new List<Sprite>();
    private readonly List<Texture2D> generatedTextures = new List<Texture2D>();
    private readonly List<Material> generatedMaterials = new List<Material>();

    private float elapsed;
    private float totalDuration;
    private float iconSize;

    private static readonly Color Aurora = new Color(0.12f, 0.94f, 1f, 1f);
    private static readonly Color IceBlue = new Color(0.24f, 0.68f, 1f, 1f);
    private static readonly Color CobaltRim = new Color(0.07f, 0.2f, 0.62f, 1f);
    private static readonly Color SnowWhite = new Color(0.96f, 1f, 1f, 1f);
    private static readonly Color VioletIce = new Color(0.55f, 0.74f, 1f, 0.96f);

    public static void Play(Sprite icon, Vector3 position, SpriteRenderer source)
    {
        if (icon == null)
            return;

        GameObject burst = new GameObject("DoubleJumpPickupBurst_IceBloom");
        burst.transform.position = position;
        DoubleJumpPickupBurst animation = burst.AddComponent<DoubleJumpPickupBurst>();
        animation.Build(icon, source);
    }

    private void Build(Sprite icon, SpriteRenderer source)
    {
        iconSize = Mathf.Clamp(Mathf.Max(icon.bounds.size.x, icon.bounds.size.y), 0.6f, 1.25f);
        int sortingLayer = source != null ? source.sortingLayerID : 0;
        int order = source != null ? source.sortingOrder + 8 : 20;
        Material material = CreateEffectMaterial(source);

        Sprite glow = CreateGlowSprite(128);
        Sprite ring = CreateRingSprite(128);
        Sprite sparkle = CreateSparkleSprite(64);
        Sprite shard = CreateShardSprite(64);

        AddElement("AuroraBloom", glow, material, sortingLayer, order - 3,
            ColorForAlpha(Aurora, 0.72f), Vector3.zero, iconSize * 0.3f, iconSize * 2.9f,
            0f, 0.68f, MotionStyle.Expand, 0f, 0f, 0f, 0f, Vector2.zero);
        AddElement("CoreFlash", glow, material, sortingLayer, order - 1,
            ColorForAlpha(SnowWhite, 1f), Vector3.zero, iconSize * 0.24f, iconSize * 1.2f,
            0f, 0.34f, MotionStyle.Expand, 0f, 0f, 0f, 0f, Vector2.zero);
        AddElement("CobaltRim", ring, material, sortingLayer, order - 1,
            ColorForAlpha(CobaltRim, 0.96f), Vector3.zero, iconSize * 0.54f, iconSize * 2.7f,
            0.015f, 0.66f, MotionStyle.Expand, 0f, 0f, -4f, -84f, Vector2.zero);
        AddElement("OuterIceRing", ring, material, sortingLayer, order,
            ColorForAlpha(Aurora, 1f), Vector3.zero, iconSize * 0.48f, iconSize * 2.55f,
            0.015f, 0.66f, MotionStyle.Expand, 0f, 0f, 4f, 84f, Vector2.zero);
        AddElement("InnerIceRing", ring, material, sortingLayer, order - 2,
            ColorForAlpha(VioletIce, 1f), Vector3.zero, iconSize * 0.3f, iconSize * 1.78f,
            0.045f, 0.72f, MotionStyle.Expand, 0f, 0f, -8f, -68f, Vector2.zero);

        AddElement("CollectedIcon", icon, material, sortingLayer, order + 2,
            SnowWhite, Vector3.zero, iconSize * 0.42f, iconSize * 1.08f,
            0f, 0.56f, MotionStyle.IconPop, 0f, 0f, 0f, -42f, Vector2.zero);
        AddElement("CoreSnowSparkle", sparkle, material, sortingLayer, order + 3,
            SnowWhite, Vector3.zero, iconSize * 0.12f, iconSize * 0.42f,
            0.02f, 0.42f, MotionStyle.Expand, 0f, 0f, 0f, 24f, Vector2.zero);

        var random = new System.Random(GetInstanceID() ^ Time.frameCount);
        Color[] shardColors = { SnowWhite, Aurora, IceBlue, VioletIce };
        for (int i = 0; i < 36; i++)
        {
            float angle = (float)(random.NextDouble() * Math.PI * 2.0);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            bool isShard = i < 12;
            float duration = RandomRange(random, 0.48f, 0.82f);
            float size = iconSize * (isShard
                ? RandomRange(random, 0.095f, 0.15f)
                : RandomRange(random, 0.06f, 0.105f));
            Color tint = shardColors[random.Next(shardColors.Length)];
            Sprite particleSprite = isShard ? shard : sparkle;

            AddElement(isShard ? "IceShard" : "SnowSpark", particleSprite, material,
                sortingLayer, order + 3, tint, direction * (iconSize * 0.08f),
                size, size * RandomRange(random, 0.18f, 0.42f),
                RandomRange(random, 0f, 0.055f), duration, MotionStyle.Shard,
                RandomRange(random, 1.8f, 3.7f), RandomRange(random, 1.2f, 2.8f),
                (float)(random.NextDouble() * 360.0), RandomRange(random, -360f, 360f),
                direction);
        }

        totalDuration = 0.9f;
    }

    private void AddElement(
        string name,
        Sprite sprite,
        Material material,
        int sortingLayer,
        int sortingOrder,
        Color color,
        Vector3 startPosition,
        float startScale,
        float endScale,
        float startDelay,
        float duration,
        MotionStyle style,
        float speed,
        float gravity,
        float startRotation,
        float rotationSpeed,
        Vector2 direction)
    {
        GameObject elementObject = new GameObject(name);
        Transform elementTransform = elementObject.transform;
        elementTransform.SetParent(transform, false);
        elementTransform.localPosition = startPosition;
        elementTransform.localScale = Vector3.one * startScale;

        SpriteRenderer spriteRenderer = elementObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sharedMaterial = material;
        spriteRenderer.sortingLayerID = sortingLayer;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = color;
        spriteRenderer.enabled = true;

        elements.Add(new BurstElement
        {
            transform = elementTransform,
            renderer = spriteRenderer,
            color = color,
            startPosition = startPosition,
            direction = direction,
            startScale = startScale,
            endScale = endScale,
            startDelay = startDelay,
            duration = Mathf.Max(0.01f, duration),
            speed = speed,
            gravity = gravity,
            startRotation = startRotation,
            rotationSpeed = rotationSpeed,
            style = style
        });
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        for (int i = 0; i < elements.Count; i++)
        {
            BurstElement element = elements[i];
            if (element.renderer == null)
                continue;

            float age = elapsed - element.startDelay;
            if (age < 0f)
            {
                element.renderer.enabled = false;
                continue;
            }

            element.renderer.enabled = true;
            float t = Mathf.Clamp01(age / element.duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float scale = Mathf.Lerp(element.startScale, element.endScale, eased);
            float alpha;

            if (element.style == MotionStyle.Shard)
            {
                float travelTime = Mathf.Min(age, element.duration);
                Vector3 travel = (Vector3)(element.direction * element.speed * travelTime);
                travel.y -= element.gravity * travelTime * travelTime * 0.5f;
                element.transform.localPosition = element.startPosition + travel;
                scale *= 1f - t * 0.72f;
                alpha = Mathf.Pow(1f - t, 1.15f);
            }
            else if (element.style == MotionStyle.IconPop)
            {
                float pulse = 0.72f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                scale = element.endScale * pulse;
                alpha = Mathf.Pow(1f - t, 0.7f);
            }
            else
            {
                alpha = Mathf.Pow(1f - t, 1.08f);
            }

            element.transform.localScale = Vector3.one * scale;
            element.transform.localRotation = Quaternion.Euler(
                0f, 0f, element.startRotation + element.rotationSpeed * age);
            Color color = element.color;
            color.a *= alpha;
            element.renderer.color = color;
        }

        if (elapsed >= totalDuration)
            Destroy(gameObject);
    }

    private Sprite CreateGlowSprite(int size)
    {
        return CreateSprite("IceBloom_Glow", size, (x, y) =>
        {
            float px = (x + 0.5f) / size * 2f - 1f;
            float py = (y + 0.5f) / size * 2f - 1f;
            float radius = Mathf.Sqrt(px * px + py * py);
            float openCenter = Mathf.SmoothStep(0.18f, 0.44f, radius);
            float radial = Mathf.Pow(Mathf.Clamp01(1f - radius), 1.8f) * openCenter;
            float angle = Mathf.Atan2(py, px);
            float rays = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 10f);
            byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radial * (0.58f + rays * 0.55f)) * 255f);
            return new Color32(255, 255, 255, alpha);
        });
    }

    private Sprite CreateRingSprite(int size)
    {
        return CreateSprite("IceBloom_Ring", size, (x, y) =>
        {
            float px = (x + 0.5f) / size * 2f - 1f;
            float py = (y + 0.5f) / size * 2f - 1f;
            float radius = Mathf.Sqrt(px * px + py * py);
            float line = 1f - Mathf.SmoothStep(0.66f, 0.79f, radius);
            line *= Mathf.SmoothStep(0.6f, 0.7f, radius);
            float halo = Mathf.Exp(-Mathf.Pow((radius - 0.72f) * 10f, 2f)) * 0.24f;
            byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(Mathf.Max(line, halo)) * 255f);
            return new Color32(255, 255, 255, alpha);
        });
    }

    private Sprite CreateSparkleSprite(int size)
    {
        return CreateSprite("IceBloom_Sparkle", size, (x, y) =>
        {
            float px = (x + 0.5f) / size * 2f - 1f;
            float py = (y + 0.5f) / size * 2f - 1f;
            float radius = Mathf.Sqrt(px * px + py * py);
            float angle = Mathf.Atan2(py, px);
            float tip = 0.12f + 0.78f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2f)), 9f);
            float alphaValue = 1f - Mathf.SmoothStep(tip - 0.055f, tip + 0.025f, radius);
            byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(alphaValue) * 255f);
            return new Color32(255, 255, 255, alpha);
        });
    }

    private Sprite CreateShardSprite(int size)
    {
        return CreateSprite("IceBloom_Shard", size, (x, y) =>
        {
            float px = Mathf.Abs((x + 0.5f) / size * 2f - 1f);
            float py = Mathf.Abs((y + 0.5f) / size * 2f - 1f);
            float diamond = px / 0.5f + py / 0.92f;
            float alphaValue = 1f - Mathf.SmoothStep(0.86f, 1.04f, diamond);
            byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(alphaValue) * 255f);
            return new Color32(255, 255, 255, alpha);
        });
    }

    private Sprite CreateSprite(string name, int size, Func<int, int, Color32> pixel)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = name + "Texture";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = pixel(x, y);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        generatedTextures.Add(texture);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f), size);
        sprite.name = name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        generatedSprites.Add(sprite);
        return sprite;
    }

    private Material CreateEffectMaterial(SpriteRenderer source)
    {
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader == null)
            return source != null ? source.sharedMaterial : null;

        Material material = new Material(spriteShader);
        material.name = "IceBloomEffectMaterial";
        material.hideFlags = HideFlags.HideAndDontSave;
        generatedMaterials.Add(material);
        return material;
    }

    private static Color ColorForAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static float RandomRange(System.Random random, float minimum, float maximum)
    {
        return minimum + (float)random.NextDouble() * (maximum - minimum);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < generatedMaterials.Count; i++)
        {
            if (generatedMaterials[i] != null)
                Destroy(generatedMaterials[i]);
        }

        for (int i = 0; i < generatedSprites.Count; i++)
        {
            if (generatedSprites[i] != null)
                Destroy(generatedSprites[i]);
        }

        for (int i = 0; i < generatedTextures.Count; i++)
        {
            if (generatedTextures[i] != null)
                Destroy(generatedTextures[i]);
        }
    }
}
