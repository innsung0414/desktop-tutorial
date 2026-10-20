using UnityEngine;

/// <summary>
/// 화면 안내를 Canvas 기반 uGUI로 표시합니다.
/// 낚시 규칙은 FishingGameController, 익사 규칙은 DrowningController가 담당합니다.
/// </summary>
public class FishingHUD : MonoBehaviour
{
    [SerializeField] private FishingGameController fishingController;
    [SerializeField] private DrowningController drowningController;
    [SerializeField, Range(0f, 1f)] private float feedbackVolume = 0.28f;

    private static readonly Color PanelColor = new Color(0.015f, 0.075f, 0.10f, 0.92f);
    private static readonly Color PanelEdgeColor = new Color(0.18f, 0.55f, 0.60f, 0.92f);
    private static readonly Color AccentColor = new Color(0.28f, 0.88f, 0.88f, 1f);
    private static readonly Color GoldColor = new Color(1f, 0.78f, 0.30f, 1f);
    private static readonly Color DangerColor = new Color(1f, 0.34f, 0.27f, 1f);

    private GameObject mainPanel;
    private GameObject chargeGroup;
    private GameObject reelGroup;
    private GameObject bitePanel;
    private GameObject feedbackPanel;
    private GameObject resultPanel;
    private GameObject drowningPanel;
    private GameObject deathOverlay;
    private GameObject crosshair;

    private UnityEngine.UI.Text promptText;
    private UnityEngine.UI.Text biteText;
    private UnityEngine.UI.Text feedbackText;
    private UnityEngine.UI.Text resultTitleText;
    private UnityEngine.UI.Text resultMessageText;
    private UnityEngine.UI.Text fishNameText;
    private UnityEngine.UI.Text drowningText;

    private UnityEngine.UI.Image castFill;
    private UnityEngine.UI.Image reelFill;
    private UnityEngine.UI.Image tensionFill;
    private UnityEngine.UI.Image feedbackBackground;
    private UnityEngine.UI.Image feedbackAccent;
    private UnityEngine.UI.Image resultFishIcon;
    private UnityEngine.CanvasGroup feedbackCanvasGroup;
    private Font koreanFont;
    private bool ownsKoreanFont;
    private Canvas hudCanvas;
    private AudioSource feedbackAudioSource;
    private AudioClip splashClip;
    private AudioClip biteClip;
    private AudioClip hookClip;
    private AudioClip successClip;
    private AudioClip failureClip;
    private float feedbackUntil;
    private bool wasWaitingForBite;
    private bool wasBiteActive;
    private bool wasReeling;
    private bool wasShowingResult;

    private void Awake()
    {
        BindControllers();
        BuildCanvas();
    }

    private void Start()
    {
        BindControllers();
        RefreshHud();
    }

    private void Update()
    {
        if (hudCanvas != null) hudCanvas.enabled = !DeepBlueMenus.IsMenuOpen;
        if (DeepBlueMenus.IsMenuOpen) return;
        if (fishingController == null || drowningController == null)
        {
            BindControllers();
        }

        RefreshHud();
    }

    private void BindControllers()
    {
        if (fishingController == null)
        {
            fishingController = GetComponent<FishingGameController>();
        }

        if (drowningController == null)
        {
            drowningController = FindFirstObjectByType<DrowningController>();
        }
    }

    private void BuildCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Deep Blue HUD Canvas",
            typeof(RectTransform),
            typeof(UnityEngine.Canvas),
            typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        UnityEngine.Canvas canvas = canvasObject.GetComponent<UnityEngine.Canvas>();
        hudCanvas = canvas;
        canvas.enabled = !DeepBlueMenus.IsMenuOpen;
        canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        // The default Windows build opens at 1280x720, so keep the authored text
        // sizes readable at the project's standard resolution.
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        koreanFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 24);
        ownsKoreanFont = koreanFont != null;
        if (koreanFont == null)
        {
            koreanFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        BuildAudioFeedback();
        BuildFishingPanel(canvasObject.transform);
        BuildCrosshair(canvasObject.transform);
        BuildBiteAlert(canvasObject.transform);
        BuildFeedbackBanner(canvasObject.transform);
        BuildResultPanel(canvasObject.transform);
        BuildDrowningPanel(canvasObject.transform);
        BuildDeathOverlay(canvasObject.transform);
    }

    private void BuildFishingPanel(Transform parent)
    {
        mainPanel = CreatePanel("Fishing Status Panel", parent, PanelColor,
            Vector2.zero, Vector2.zero, new Vector2(24f, 24f), new Vector2(500f, 232f));
        CreateImage("Panel Accent", mainPanel.transform, PanelEdgeColor, Vector2.zero, new Vector2(4f, 232f));

        AddText("Title", mainPanel.transform, "DEEP BLUE  ·  낚시", 22, FontStyle.Bold,
            new Color(0.68f, 0.95f, 0.96f, 1f), TextAnchor.MiddleLeft,
            new Vector2(18f, -16f), new Vector2(464f, 36f));

        promptText = AddText("Fishing Prompt", mainPanel.transform, "마우스 왼쪽 버튼을 눌렀다 놓아 던지기", 18, FontStyle.Normal,
            Color.white, TextAnchor.MiddleLeft, new Vector2(18f, -54f), new Vector2(464f, 48f));

        chargeGroup = CreateRectObject("Cast Charge Group", mainPanel.transform,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -112f), new Vector2(464f, 64f));
        AddText("Cast Label", chargeGroup.transform, "캐스팅 힘", 14, FontStyle.Bold, AccentColor,
            TextAnchor.MiddleLeft, Vector2.zero, new Vector2(464f, 22f));
        castFill = CreateProgressBar("Cast Charge Bar", chargeGroup.transform, new Vector2(0f, -31f), new Vector2(464f, 14f), AccentColor);

        reelGroup = CreateRectObject("Reeling Group", mainPanel.transform,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -112f), new Vector2(464f, 76f));
        AddText("Reel Label", reelGroup.transform, "릴 진행", 14, FontStyle.Bold, AccentColor,
            TextAnchor.MiddleLeft, Vector2.zero, new Vector2(86f, 22f));
        reelFill = CreateProgressBar("Reel Progress Bar", reelGroup.transform, new Vector2(92f, -4f), new Vector2(372f, 14f), AccentColor);
        AddText("Tension Label", reelGroup.transform, "줄 장력", 14, FontStyle.Bold, GoldColor,
            TextAnchor.MiddleLeft, new Vector2(0f, -38f), new Vector2(86f, 22f));
        tensionFill = CreateProgressBar("Line Tension Bar", reelGroup.transform, new Vector2(92f, -42f), new Vector2(372f, 14f), GoldColor);

        AddText("Escape Hint", mainPanel.transform, "WASD 이동 · 마우스 시점 · ESC 메뉴 · TAB 도감", 14, FontStyle.Normal,
            new Color(0.72f, 0.82f, 0.84f, 1f), TextAnchor.MiddleLeft,
            new Vector2(18f, -204f), new Vector2(464f, 20f));
    }

    private void BuildCrosshair(Transform parent)
    {
        crosshair = CreateRectObject("Crosshair", parent,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(38f, 38f));
        AddText("Crosshair Mark", crosshair.transform, "+", 30, FontStyle.Normal,
            new Color(0.88f, 0.98f, 0.98f, 0.9f), TextAnchor.MiddleCenter, Vector2.zero, new Vector2(38f, 38f));
    }

    private void BuildBiteAlert(Transform parent)
    {
        bitePanel = CreatePanel("Bite Alert", parent, PanelColor,
            new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), Vector2.zero, new Vector2(540f, 92f));
        biteText = AddText("Bite Alert Text", bitePanel.transform, "입질! Space 또는 왼쪽 버튼으로 챔질하세요", 26, FontStyle.Bold,
            GoldColor, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(504f, 84f), true);
    }

    private void BuildFeedbackBanner(Transform parent)
    {
        feedbackPanel = CreatePanel("Fishing Event Feedback", parent, PanelColor,
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(660f, 76f));
        feedbackBackground = feedbackPanel.GetComponent<UnityEngine.UI.Image>();
        feedbackCanvasGroup = feedbackPanel.AddComponent<UnityEngine.CanvasGroup>();
        feedbackAccent = CreateImage("Feedback Accent", feedbackPanel.transform, AccentColor,
            new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(5f, 76f));
        feedbackText = AddText("Feedback Message", feedbackPanel.transform, string.Empty, 23, FontStyle.Bold,
            Color.white, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(620f, 68f), true);
        feedbackPanel.SetActive(false);
    }

    private void BuildResultPanel(Transform parent)
    {
        resultPanel = CreatePanel("Fishing Result Panel", parent, PanelColor,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 274f));

        resultTitleText = AddText("Result Title", resultPanel.transform, "낚시 결과", 25, FontStyle.Bold,
            new Color(0.68f, 0.95f, 0.96f, 1f), TextAnchor.MiddleLeft,
            new Vector2(24f, -16f), new Vector2(572f, 38f));

        resultFishIcon = CreateImage("Caught Fish Icon", resultPanel.transform, Color.white,
            new Vector2(24f, -68f), new Vector2(158f, 142f));
        resultFishIcon.preserveAspect = true;

        fishNameText = AddText("Fish Name", resultPanel.transform, string.Empty, 24, FontStyle.Bold,
            GoldColor, TextAnchor.MiddleLeft, new Vector2(202f, -80f), new Vector2(390f, 46f));
        resultMessageText = AddText("Result Message", resultPanel.transform, string.Empty, 16, FontStyle.Normal,
            Color.white, TextAnchor.UpperLeft, new Vector2(202f, -132f), new Vector2(390f, 66f));
        AddText("Retry Hint", resultPanel.transform, "마우스 왼쪽 버튼을 눌러 다시 낚시", 16, FontStyle.Normal,
            new Color(0.76f, 0.87f, 0.88f, 1f), TextAnchor.MiddleLeft,
            new Vector2(24f, -226f), new Vector2(572f, 28f));
    }

    private void BuildDrowningPanel(Transform parent)
    {
        drowningPanel = CreatePanel("Drowning Timer Panel", parent, PanelColor,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(300f, 78f));
        drowningText = AddText("Drowning Timer Text", drowningPanel.transform, "익사까지 10.0초", 20, FontStyle.Bold,
            DangerColor, TextAnchor.MiddleCenter, new Vector2(12f, -8f), new Vector2(276f, 62f));
    }

    private void BuildDeathOverlay(Transform parent)
    {
        deathOverlay = CreateImage("Drowned Screen Overlay", parent, new Color(0.005f, 0.025f, 0.04f, 0.84f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
        AddText("Drowned Title", deathOverlay.transform, "익사했습니다", 42, FontStyle.Bold,
            new Color(1f, 0.83f, 0.78f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 34f), new Vector2(760f, 82f), true);
        AddText("Drowned Message", deathOverlay.transform, "물에서 벗어나지 못해 낚시가 종료되었습니다.", 19, FontStyle.Normal,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, -28f), new Vector2(760f, 46f), true);
        deathOverlay.SetActive(false);
    }

    private void RefreshHud()
    {
        if (mainPanel == null)
        {
            return;
        }

        bool dead = drowningController != null && drowningController.IsDead;
        bool inWater = drowningController != null && drowningController.IsInWater;
        bool hasResult = fishingController != null && fishingController.IsResultVisible;
        // The result card already explains the outcome and how to try again.
        mainPanel.SetActive(!dead && !hasResult);

        if (fishingController != null)
        {
            bool waitingForBite = fishingController.IsWaitingForBite;
            bool biteActive = fishingController.IsBiteActive;
            bool reeling = fishingController.IsReeling;
            bool showingResult = fishingController.IsResultVisible;

            if (waitingForBite && !wasWaitingForBite)
            {
                ShowFeedback("찌가 물에 닿았습니다. 입질을 기다리세요.", AccentColor, 1.8f);
                PlayFeedbackSound(splashClip);
            }

            if (reeling && !wasReeling)
            {
                ShowFeedback("챔질 성공! 왼쪽 버튼을 눌러 감으세요.", AccentColor, 1.8f);
                PlayFeedbackSound(hookClip);
            }

            if (showingResult && !wasShowingResult)
            {
                string resultFeedback = fishingController.DidCatchFish
                    ? "낚시 성공! 결과를 확인하세요."
                    : "낚시에 실패했습니다.";
                ShowFeedback(resultFeedback, fishingController.DidCatchFish ? GoldColor : DangerColor, 2.2f);
                PlayFeedbackSound(fishingController.DidCatchFish ? successClip : failureClip);
            }

            if (biteActive && !wasBiteActive)
            {
                bitePanel.transform.localScale = Vector3.one * 0.88f;
                PlayFeedbackSound(biteClip);
            }

            wasWaitingForBite = waitingForBite;
            wasBiteActive = biteActive;
            wasReeling = reeling;
            wasShowingResult = showingResult;

            promptText.text = fishingController.PromptText;

            chargeGroup.SetActive(fishingController.IsCharging && !dead);
            SetProgress(castFill, fishingController.CastChargeNormalized);

            reelGroup.SetActive(fishingController.IsReeling && !dead);
            SetProgress(reelFill, fishingController.ReelProgress);
            SetProgress(tensionFill, fishingController.LineTension);
            tensionFill.color = fishingController.LineTension >= 0.7f ? DangerColor : GoldColor;

            bitePanel.SetActive(fishingController.IsBiteActive && !dead);
            if (fishingController.IsBiteActive)
            {
                bitePanel.transform.localScale = Vector3.Lerp(
                    bitePanel.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 12f);
            }
            biteText.text = $"입질! {Mathf.CeilToInt(fishingController.BiteSecondsRemaining)}초 안에 Space 또는 왼쪽 클릭";

            resultPanel.SetActive(fishingController.IsResultVisible && !dead);
            resultTitleText.text = fishingController.ResultTitle;
            resultMessageText.text = fishingController.ResultMessage;
            resultMessageText.rectTransform.anchoredPosition = fishingController.DidCatchFish
                ? new Vector2(202f, -132f) : new Vector2(24f, -90f);
            resultMessageText.rectTransform.sizeDelta = fishingController.DidCatchFish
                ? new Vector2(390f, 66f) : new Vector2(572f, 100f);
            fishNameText.text = fishingController.DidCatchFish ? fishingController.ResultFishName : string.Empty;
            resultFishIcon.sprite = fishingController.DidCatchFish ? fishingController.ResultFishIcon : null;
            resultFishIcon.enabled = fishingController.DidCatchFish && resultFishIcon.sprite != null;
        }

        drowningPanel.SetActive(inWater && !dead);
        if (inWater && !dead)
        {
            drowningText.text = $"익사까지 {drowningController.SecondsRemaining:0.0}초";
        }

        crosshair.SetActive(!dead && !hasResult);
        deathOverlay.SetActive(dead);

        bool showFeedback = !dead && Time.unscaledTime < feedbackUntil;
        feedbackPanel.SetActive(showFeedback);
        if (showFeedback)
        {
            feedbackCanvasGroup.alpha = Mathf.Clamp01((feedbackUntil - Time.unscaledTime) / 0.3f);
            feedbackPanel.transform.localScale = Vector3.Lerp(
                feedbackPanel.transform.localScale, Vector3.one, Time.unscaledDeltaTime * 12f);
        }
    }

    private void ShowFeedback(string message, Color accentColor, float duration)
    {
        feedbackText.text = message;
        feedbackText.color = Color.white;
        feedbackAccent.color = accentColor;
        feedbackBackground.color = PanelColor;
        feedbackUntil = Time.unscaledTime + duration;
        feedbackCanvasGroup.alpha = 1f;
        feedbackPanel.transform.localScale = Vector3.one * 0.92f;
        feedbackPanel.SetActive(true);
    }

    private void BuildAudioFeedback()
    {
        feedbackAudioSource = GetComponent<AudioSource>();
        if (feedbackAudioSource == null)
        {
            feedbackAudioSource = gameObject.AddComponent<AudioSource>();
        }

        feedbackAudioSource.playOnAwake = false;
        feedbackAudioSource.spatialBlend = 0f;
        feedbackAudioSource.volume = feedbackVolume;

        // 짧은 프로토타입 효과음을 코드로 한 번 생성해 별도 오디오 자산 없이 사용합니다.
        splashClip = CreateSplashClip();
        biteClip = CreateToneSequence("Bite Cue", new[] { 880f, 1174.7f }, 0.12f, 0.055f);
        hookClip = CreateToneSequence("Hook Cue", new[] { 587.3f, 783.99f }, 0.07f, 0.015f);
        successClip = CreateToneSequence("Catch Success Cue", new[] { 523.25f, 659.25f, 783.99f }, 0.17f, 0.025f);
        failureClip = CreateToneSequence("Catch Failure Cue", new[] { 392f, 311.13f, 233.08f }, 0.18f, 0.04f);
    }

    private void PlayFeedbackSound(AudioClip clip)
    {
        if (feedbackAudioSource != null && clip != null)
        {
            feedbackAudioSource.PlayOneShot(clip);
        }
    }

    private static AudioClip CreateSplashClip()
    {
        const int sampleRate = 22050;
        const float duration = 0.48f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        System.Random random = new System.Random(42);
        float filteredNoise = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float noise = (float)(random.NextDouble() * 2.0 - 1.0);
            filteredNoise += 0.24f * (noise - filteredNoise);
            float envelope = Mathf.Exp(-time * 6f);
            float ripple = Mathf.Sin(2f * Mathf.PI * (360f * Mathf.Exp(-time * 2.4f)) * time);
            samples[i] = (filteredNoise * 0.62f + ripple * 0.2f) * envelope;
        }

        return CreateClip("Water Splash Cue", samples, sampleRate);
    }

    private static AudioClip CreateToneSequence(string clipName, float[] frequencies, float noteDuration, float gapDuration)
    {
        const int sampleRate = 22050;
        int noteSamples = Mathf.CeilToInt(sampleRate * noteDuration);
        int gapSamples = Mathf.CeilToInt(sampleRate * gapDuration);
        float[] samples = new float[frequencies.Length * (noteSamples + gapSamples)];

        for (int note = 0; note < frequencies.Length; note++)
        {
            int start = note * (noteSamples + gapSamples);
            for (int i = 0; i < noteSamples; i++)
            {
                float time = i / (float)sampleRate;
                float attack = Mathf.Clamp01(time / 0.012f);
                float release = Mathf.Clamp01((noteDuration - time) / 0.045f);
                float envelope = Mathf.Min(attack, release);
                samples[start + i] += Mathf.Sin(2f * Mathf.PI * frequencies[note] * time) * envelope * 0.32f;
            }
        }

        return CreateClip(clipName, samples, sampleRate);
    }

    private static AudioClip CreateClip(string clipName, float[] samples, int sampleRate)
    {
        AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (feedbackAudioSource != null)
        {
            feedbackAudioSource.Stop();
        }

        DestroyGeneratedClip(splashClip);
        DestroyGeneratedClip(biteClip);
        DestroyGeneratedClip(hookClip);
        DestroyGeneratedClip(successClip);
        DestroyGeneratedClip(failureClip);

        // Only release the font created here; the built-in fallback is shared.
        if (ownsKoreanFont && koreanFont != null)
        {
            if (Application.isPlaying) Destroy(koreanFont);
            else DestroyImmediate(koreanFont);
        }
        koreanFont = null;
        ownsKoreanFont = false;
    }

    private static void DestroyGeneratedClip(AudioClip clip)
    {
        if (clip != null)
        {
            if (Application.isPlaying)
            {
                Destroy(clip);
            }
            else
            {
                DestroyImmediate(clip);
            }
        }
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        UnityEngine.UI.Image image = CreateImage(name, parent, color, anchorMin, anchorMax, position, size);
        image.raycastTarget = false;
        return image.gameObject;
    }

    private static GameObject CreateRectObject(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        RectTransform rect = result.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return result;
    }

    private static UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color,
        Vector2 position, Vector2 size)
    {
        return CreateImage(name, parent, color, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
    }

    private static UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject imageObject = CreateRectObject(name, parent, anchorMin, anchorMax, position, size);
        UnityEngine.UI.Image image = imageObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private UnityEngine.UI.Text AddText(string name, Transform parent, string value, int fontSize,
        FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 position, Vector2 size, bool centered = false)
    {
        Vector2 anchor = centered ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 1f);
        GameObject textObject = CreateRectObject(name, parent, anchor, anchor, position, size);
        UnityEngine.UI.Text text = textObject.AddComponent<UnityEngine.UI.Text>();
        text.font = koreanFont;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static UnityEngine.UI.Image CreateProgressBar(string name, Transform parent, Vector2 position,
        Vector2 size, Color fillColor)
    {
        UnityEngine.UI.Image background = CreateImage(name, parent, new Color(0.08f, 0.16f, 0.19f, 1f), position, size);
        UnityEngine.UI.Image fill = CreateImage("Fill", background.transform, fillColor,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;
        return fill;
    }

    private static void SetProgress(UnityEngine.UI.Image fill, float amount)
    {
        if (fill == null)
        {
            return;
        }

        float progress = Mathf.Clamp01(amount);
        RectTransform rect = fill.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(progress, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
