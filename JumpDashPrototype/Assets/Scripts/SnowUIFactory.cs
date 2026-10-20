using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class SnowUIFactory
{
    public static readonly Color Navy = new Color(0.025f, 0.085f, 0.16f, 0.92f);
    public static readonly Color Cyan = new Color(0.25f, 0.88f, 0.96f, 1f);
    public static readonly Color Ice = new Color(0.88f, 0.97f, 1f, 1f);
    public static readonly Color MutedIce = new Color(0.61f, 0.79f, 0.88f, 1f);

    private static TMP_FontAsset cachedFont;
    private static Sprite cachedRoundedSprite;

    public static TMP_FontAsset Font
    {
        get
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.Load<TMP_FontAsset>("Font/SUIT-otf/SUIT-Bold SDF");
                if (cachedFont == null)
                    cachedFont = TMP_Settings.defaultFontAsset;
            }

            return cachedFont;
        }
    }

    public static void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    public static Image CreatePanel(
        Transform parent,
        string objectName,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 size,
        Vector2 position,
        Color color)
    {
        GameObject panelObject = CreateUIObject(objectName, parent, typeof(Image));
        Image image = panelObject.GetComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        SetRect(panelObject.GetComponent<RectTransform>(), anchor, pivot, size, position);
        return image;
    }

    public static Image CreateImage(Transform parent, string objectName)
    {
        GameObject imageObject = CreateUIObject(objectName, parent, typeof(Image));
        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    public static RawImage CreateRawImage(Transform parent, string objectName)
    {
        GameObject imageObject = CreateUIObject(objectName, parent, typeof(RawImage));
        RawImage image = imageObject.GetComponent<RawImage>();
        image.raycastTarget = false;
        return image;
    }

    public static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string content,
        float size,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 rectSize,
        Vector2 position,
        FontStyles style = FontStyles.Normal)
    {
        GameObject textObject = CreateUIObject(objectName, parent, typeof(TextMeshProUGUI));
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = Font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.margin = new Vector4(0f, 0f, 0f, 0f);
        SetRect(text.rectTransform, anchor, pivot, rectSize, position);
        return text;
    }

    public static Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 size,
        Vector2 position,
        UnityAction onClick)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent, typeof(Image), typeof(Button));
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = RoundedSprite;
        image.type = Image.Type.Sliced;
        image.color = Cyan;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock
        {
            normalColor = Cyan,
            highlightedColor = new Color(0.55f, 0.95f, 1f, 1f),
            pressedColor = new Color(0.13f, 0.61f, 0.73f, 1f),
            selectedColor = new Color(0.4f, 0.9f, 0.98f, 1f),
            disabledColor = new Color(0.35f, 0.52f, 0.59f, 0.55f),
            colorMultiplier = 1f,
            fadeDuration = 0.12f
        };

        SetRect(buttonObject.GetComponent<RectTransform>(), anchor, pivot, size, position);
        TextMeshProUGUI buttonText = CreateText(
            buttonObject.transform,
            "Label",
            label,
            28f,
            new Color(0.025f, 0.1f, 0.17f, 1f),
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            FontStyles.Bold);
        SetStretch(buttonText.rectTransform);

        if (onClick != null)
            button.onClick.AddListener(onClick);

        return button;
    }

    public static void SetRect(
        RectTransform rect,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 size,
        Vector2 position)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public static void SetStretch(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public static Sprite RoundedSprite
    {
        get
        {
            if (cachedRoundedSprite != null)
                return cachedRoundedSprite;

            const int textureSize = 64;
            const float radius = 15f;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.name = "Snow UI Rounded Panel";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = new Color32[textureSize * textureSize];
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float nearestX = Mathf.Clamp(x + 0.5f, radius, textureSize - radius);
                    float nearestY = Mathf.Clamp(y + 0.5f, radius, textureSize - radius);
                    float dx = x + 0.5f - nearestX;
                    float dy = y + 0.5f - nearestY;
                    pixels[y * textureSize + x] = dx * dx + dy * dy <= radius * radius
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            cachedRoundedSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                textureSize,
                0,
                SpriteMeshType.FullRect,
                new Vector4(18f, 18f, 18f, 18f));
            cachedRoundedSprite.name = "Snow UI Rounded Panel";
            return cachedRoundedSprite;
        }
    }

    private static GameObject CreateUIObject(string objectName, Transform parent, params System.Type[] components)
    {
        GameObject uiObject = new GameObject(objectName, components);
        if (parent != null)
        {
            uiObject.layer = parent.gameObject.layer;
            uiObject.transform.SetParent(parent, false);
        }

        return uiObject;
    }
}
