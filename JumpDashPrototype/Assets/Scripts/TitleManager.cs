using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class TitleManager : MonoBehaviour
{
    private const float StartFadeDuration = 0.75f;
    private static TitleManager instance;

    private CanvasGroup menuGroup;
    private CanvasGroup gameplayHudGroup;
    private TextMeshProUGUI startHint;
    private Texture2D shadeTexture;
    private bool isAttractMode;
    private bool isReady;
    private bool isStartingGame;

    public static bool IsAttractMode => instance != null && instance.isAttractMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        isAttractMode = true;
        Time.timeScale = 1f;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        if (instance != this)
            yield break;

        // Load the real course once. Pressing START later only removes the menu.
        if (SceneManager.GetActiveScene().name != "MainScene")
            yield return SceneManager.LoadSceneAsync("MainScene");

        // Let the course initialize its player, HUD and physics before adding the menu.
        yield return null;

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        Canvas gameplayCanvas = gameManager != null && gameManager.scoreText != null
            ? gameManager.scoreText.canvas
            : FindFirstObjectByType<Canvas>();
        if (gameplayCanvas == null)
        {
            Debug.LogError("The title attract mode requires the MainScene gameplay Canvas.", this);
            yield break;
        }

        gameplayHudGroup = gameplayCanvas.GetComponent<CanvasGroup>();
        if (gameplayHudGroup == null)
            gameplayHudGroup = gameplayCanvas.gameObject.AddComponent<CanvasGroup>();
        gameplayHudGroup.alpha = 0f;
        gameplayHudGroup.interactable = false;
        gameplayHudGroup.blocksRaycasts = false;

        BuildTitleOverlay();
        isReady = true;
    }

    private void Update()
    {
        if (!isReady || isStartingGame)
            return;

        if (startHint != null)
        {
            Color color = startHint.color;
            color.a = Mathf.Lerp(0.55f, 1f, (Mathf.Sin(Time.unscaledTime * 2.4f) + 1f) * 0.5f);
            startHint.color = color;
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Space))
            StartGame();
    }

    public void StartGame()
    {
        if (!isReady || isStartingGame)
            return;

        isStartingGame = true;
        StartCoroutine(FadeIntoRide());
    }

    private IEnumerator FadeIntoRide()
    {
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        double startedAt = Time.realtimeSinceStartupAsDouble;
        while (true)
        {
            float progress = Mathf.Clamp01((float)((Time.realtimeSinceStartupAsDouble - startedAt) / StartFadeDuration));
            menuGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, progress);
            gameplayHudGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 1f, progress));
            if (progress >= 1f)
                break;
            yield return null;
        }

        gameplayHudGroup.alpha = 1f;
        gameplayHudGroup.interactable = true;
        gameplayHudGroup.blocksRaycasts = true;
        isAttractMode = false;
        Destroy(gameObject);
    }

    private void BuildTitleOverlay()
    {
        GameObject overlay = new GameObject(
            "TitleOverlay", typeof(RectTransform), typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster),
            typeof(CanvasGroup));
        overlay.transform.SetParent(transform, false);
        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        SnowUIFactory.ConfigureCanvas(canvas);
        menuGroup = overlay.GetComponent<CanvasGroup>();
        Transform root = overlay.transform;

        UnityEngine.UI.RawImage shade = SnowUIFactory.CreateRawImage(root, "MenuReadabilityShade");
        shadeTexture = CreateMenuShade();
        shade.texture = shadeTexture;
        SnowUIFactory.SetStretch(shade.rectTransform);

        TextMeshProUGUI brand = SnowUIFactory.CreateText(
            root, "BrandLabel", "ALPINE SURVIVAL  /  ENDLESS RIDE",
            21f, SnowUIFactory.Ice, TextAlignmentOptions.Left,
            new Vector2(0.055f, 0.935f), new Vector2(0f, 0.5f),
            new Vector2(800f, 40f), Vector2.zero, FontStyles.Bold);
        brand.characterSpacing = 2f;

        TextMeshProUGUI title = SnowUIFactory.CreateText(
            root, "GameTitle", "<color=#F5FCFF>JUMP</color>\n<color=#69E8F6>DASH</color>",
            126f, SnowUIFactory.Ice, TextAlignmentOptions.Left,
            new Vector2(0.055f, 0.87f), new Vector2(0f, 1f),
            new Vector2(740f, 305f), Vector2.zero, FontStyles.Bold);
        title.lineSpacing = -15f;

        BuildTitleLeaderboard(root);

        UnityEngine.UI.Image startPanel = SnowUIFactory.CreatePanel(
            root, "StartControls",
            new Vector2(0.75f, 0.48f), new Vector2(0.5f, 0.5f),
            new Vector2(510f, 246f), Vector2.zero,
            new Color(SnowUIFactory.Navy.r, SnowUIFactory.Navy.g, SnowUIFactory.Navy.b, 0.84f));
        SnowUIFactory.CreateText(
            startPanel.transform, "FreeRideLabel", "FREE RIDE  /  YOUR RUN IS NEXT",
            18f, SnowUIFactory.Ice, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.5f),
            new Vector2(470f, 36f), Vector2.zero, FontStyles.Bold);

        UnityEngine.UI.Button startButton = SnowUIFactory.CreateButton(
            startPanel.transform, "StartButton", "START RIDING   ›",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(430f, 88f), Vector2.zero, StartGame);

        startHint = SnowUIFactory.CreateText(
            startPanel.transform, "StartHint", "PRESS ENTER OR SPACE",
            18f, SnowUIFactory.Ice, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.5f),
            new Vector2(470f, 36f), Vector2.zero, FontStyles.Bold);
        startHint.characterSpacing = 1.2f;

        UnityEngine.UI.Image controls = SnowUIFactory.CreatePanel(
            root, "ControlsCard",
            new Vector2(0.5f, 0.095f), new Vector2(0.5f, 0.5f),
            new Vector2(1060f, 118f), Vector2.zero, SnowUIFactory.Navy);
        SnowUIFactory.CreateText(
            controls.transform, "ControlsHeading", "HOW TO RIDE",
            17f, SnowUIFactory.Cyan, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(360f, 30f), new Vector2(0f, -21f), FontStyles.Bold);
        TextMeshProUGUI controlsLabel = SnowUIFactory.CreateText(
            controls.transform, "ControlsLabel",
            "SPACE  JUMP     /     ITEM + SPACE  DOUBLE JUMP\nTAB  DASH     /     ESC  PAUSE",
            20f, SnowUIFactory.Ice, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(1000f, 72f), new Vector2(0f, -17f), FontStyles.Bold);
        controlsLabel.lineSpacing = 4f;

        // The stretched button label needs its mesh rebuilt after the nested layout resolves.
        Canvas.ForceUpdateCanvases();
        TextMeshProUGUI caption = startButton.GetComponentInChildren<TextMeshProUGUI>();
        caption.ForceMeshUpdate(true, true);
        caption.SetAllDirty();
    }

    private static void BuildTitleLeaderboard(Transform parent)
    {
        UnityEngine.UI.Image card = SnowUIFactory.CreatePanel(
            parent, "TitleTopFiveCard",
            Vector2.one, Vector2.one,
            new Vector2(420f, 282f), new Vector2(-56f, -58f),
            new Color(SnowUIFactory.Navy.r, SnowUIFactory.Navy.g, SnowUIFactory.Navy.b, 0.9f));

        SnowUIFactory.CreateText(
            card.transform, "TitleTopFiveHeading", "LOCAL ARCADE TOP 5",
            18f, SnowUIFactory.Cyan, TextAlignmentOptions.Left,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(374f, 30f), new Vector2(22f, -17f), FontStyles.Bold);

        List<ArcadeLeaderboard.ScoreEntry> scores = ArcadeLeaderboard.Load();
        for (int i = 0; i < ArcadeLeaderboard.MaxEntries; i++)
        {
            bool hasScore = i < scores.Count;
            Color color = i == 0 ? SnowUIFactory.Ice : SnowUIFactory.MutedIce;
            float y = -65f - i * 39f;
            SnowUIFactory.CreateText(
                card.transform, "TitleTopFiveRank_" + (i + 1), (i + 1).ToString("00"),
                19f, SnowUIFactory.Cyan, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(45f, 30f), new Vector2(24f, y), FontStyles.Bold);
            TextMeshProUGUI nickname = SnowUIFactory.CreateText(
                card.transform, "TitleTopFiveName_" + (i + 1), hasScore ? scores[i].nickname : "---",
                19f, color, TextAlignmentOptions.Left,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(218f, 30f), new Vector2(76f, y), FontStyles.Bold);
            nickname.richText = false;
            SnowUIFactory.CreateText(
                card.transform, "TitleTopFiveTime_" + (i + 1), hasScore ? FormatDuration(scores[i].scoreSeconds) : "--:--",
                19f, color, TextAlignmentOptions.Right,
                Vector2.one, Vector2.one,
                new Vector2(80f, 30f), new Vector2(-22f, y), FontStyles.Bold);
        }
    }

    private static Texture2D CreateMenuShade()
    {
        const int height = 128;
        Texture2D texture = new Texture2D(1, height, TextureFormat.RGBA32, false);
        texture.name = "Title readability shade";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] colors = new Color[height];
        for (int y = 0; y < height; y++)
        {
            float alpha = Mathf.SmoothStep(0f, 0.62f, Mathf.InverseLerp(0.28f, 1f, y / (float)(height - 1)));
            colors[y] = new Color(0.012f, 0.04f, 0.09f, alpha);
        }
        texture.SetPixels(colors);
        texture.Apply(false, true);
        return texture;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
        if (shadeTexture != null)
            Destroy(shadeTexture);
    }

    private static string FormatDuration(int seconds)
    {
        return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
    }
}

