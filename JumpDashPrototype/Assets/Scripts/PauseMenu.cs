using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public sealed class PauseMenu : MonoBehaviour
{
    private const string VolumePreference = "JumpDash.MasterVolume";
    private const string FullscreenPreference = "JumpDash.Fullscreen";

    private GameManager gameManager;
    private GameObject overlayBackdrop;
    private GameObject pausePage;
    private GameObject settingsPage;
    private TextMeshProUGUI volumeValue;
    private float previousTimeScale = 1f;
    private bool isPaused;
    private bool isReady;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
        BuildMenu();

        float volume = PlayerPrefs.GetFloat(VolumePreference, AudioListener.volume);
        ApplyMasterVolume(volume);

        bool fullscreen = PlayerPrefs.GetInt(FullscreenPreference, Screen.fullScreen ? 1 : 0) == 1;
        ApplyFullscreen(fullscreen, false);
        isReady = true;
    }

    private void Update()
    {
        if (!isReady || gameManager == null || gameManager.IsGameOver || TitleManager.IsAttractMode)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (isPaused && settingsPage.activeSelf)
        {
            ShowPausePage();
            return;
        }

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void BuildMenu()
    {
        GameObject canvasObject = new GameObject(
            "PauseMenuCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        SnowUIFactory.ConfigureCanvas(canvas);

        UnityEngine.UI.Image backdrop = SnowUIFactory.CreateImage(canvas.transform, "PauseBackdrop");
        SnowUIFactory.SetStretch(backdrop.rectTransform);
        backdrop.color = new Color(0.012f, 0.045f, 0.09f, 0.78f);
        backdrop.raycastTarget = true;
        overlayBackdrop = backdrop.gameObject;

        BuildPausePage(canvas.transform);
        BuildSettingsPage(canvas.transform);
        overlayBackdrop.SetActive(false);
        settingsPage.SetActive(false);
        pausePage.SetActive(false);
    }

    private void BuildPausePage(Transform parent)
    {
        UnityEngine.UI.Image panel = SnowUIFactory.CreatePanel(
            parent,
            "PausePanel",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(760f, 700f),
            Vector2.zero,
            SnowUIFactory.Navy);
        pausePage = panel.gameObject;

        TextMeshProUGUI heading = SnowUIFactory.CreateText(
            panel.transform, "PauseHeading", "GAME PAUSED",
            52f, SnowUIFactory.Ice, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(680f, 70f), new Vector2(0f, -78f), FontStyles.Bold);
        heading.characterSpacing = 2f;

        SnowUIFactory.CreateText(
            panel.transform, "PauseSubheading", "YOUR RIDE IS SAFELY ON HOLD",
            20f, SnowUIFactory.MutedIce, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(660f, 38f), new Vector2(0f, -133f), FontStyles.Bold);

        SnowUIFactory.CreateButton(
            panel.transform, "ResumeButton", "RESUME RIDE",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(510f, 76f), new Vector2(0f, 142f), ResumeGame);
        SnowUIFactory.CreateButton(
            panel.transform, "SettingsButton", "SETTINGS",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(510f, 76f), new Vector2(0f, 42f), ShowSettingsPage);
        SnowUIFactory.CreateButton(
            panel.transform, "ReturnToTitleButton", "RETURN TO TITLE",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(510f, 76f), new Vector2(0f, -58f), ReturnToTitle);

        UnityEngine.UI.Button quitButton = SnowUIFactory.CreateButton(
            panel.transform, "QuitButton", "QUIT GAME",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(510f, 76f), new Vector2(0f, -158f), QuitGame);
        SetButtonColors(quitButton, new Color(1f, 0.59f, 0.52f, 1f));

        SnowUIFactory.CreateText(
            panel.transform, "PauseKeyboardHint", "ESC  RESUME",
            18f, SnowUIFactory.Cyan, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(460f, 34f), new Vector2(0f, 34f), FontStyles.Bold);
    }

    private void BuildSettingsPage(Transform parent)
    {
        UnityEngine.UI.Image panel = SnowUIFactory.CreatePanel(
            parent,
            "SettingsPanel",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(760f, 700f),
            Vector2.zero,
            SnowUIFactory.Navy);
        settingsPage = panel.gameObject;

        TextMeshProUGUI heading = SnowUIFactory.CreateText(
            panel.transform, "SettingsHeading", "RIDE SETTINGS",
            48f, SnowUIFactory.Ice, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(680f, 70f), new Vector2(0f, -78f), FontStyles.Bold);
        heading.characterSpacing = 1.4f;

        SnowUIFactory.CreateText(
            panel.transform, "SettingsSubheading", "CHANGES ARE SAVED AUTOMATICALLY",
            19f, SnowUIFactory.MutedIce, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(660f, 38f), new Vector2(0f, -133f), FontStyles.Bold);

        SnowUIFactory.CreateText(
            panel.transform, "VolumeLabel", "MASTER VOLUME",
            25f, SnowUIFactory.Ice, TextAlignmentOptions.Left,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 38f), new Vector2(0f, 164f), FontStyles.Bold);

        CreateVolumeSlider(panel.transform);

        volumeValue = SnowUIFactory.CreateText(
            panel.transform, "VolumeValue", "100%",
            21f, SnowUIFactory.Cyan, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 36f), new Vector2(0f, 77f), FontStyles.Bold);

        CreateFullscreenToggle(panel.transform);

        SnowUIFactory.CreateText(
            panel.transform, "SettingsControlHint", "SPACE  JUMP     TAB  DASH     ESC  BACK",
            17f, SnowUIFactory.MutedIce, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 34f), new Vector2(0f, 102f), FontStyles.Bold);

        SnowUIFactory.CreateButton(
            panel.transform, "SettingsBackButton", "BACK TO PAUSE",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(420f, 74f), new Vector2(0f, 42f), ShowPausePage);
    }

    private void CreateVolumeSlider(Transform parent)
    {
        GameObject sliderObject = new GameObject(
            "MasterVolumeSlider",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Slider));
        sliderObject.layer = parent.gameObject.layer;
        sliderObject.transform.SetParent(parent, false);
        UnityEngine.UI.Slider slider = sliderObject.GetComponent<UnityEngine.UI.Slider>();
        SnowUIFactory.SetRect(
            sliderObject.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 36f), new Vector2(0f, 119f));

        UnityEngine.UI.Image track = SnowUIFactory.CreateImage(slider.transform, "Track");
        track.color = new Color(0.11f, 0.25f, 0.34f, 1f);
        track.raycastTarget = true;
        RectTransform trackRect = track.rectTransform;
        trackRect.anchorMin = new Vector2(0f, 0.5f);
        trackRect.anchorMax = new Vector2(1f, 0.5f);
        trackRect.offsetMin = new Vector2(0f, -7f);
        trackRect.offsetMax = new Vector2(0f, 7f);

        UnityEngine.UI.Image fill = SnowUIFactory.CreateImage(track.transform, "Fill");
        fill.color = SnowUIFactory.Cyan;
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = Vector2.zero;
        fill.rectTransform.offsetMax = Vector2.zero;

        UnityEngine.UI.Image handle = SnowUIFactory.CreateImage(slider.transform, "Handle");
        handle.sprite = SnowUIFactory.RoundedSprite;
        handle.color = SnowUIFactory.Ice;
        handle.raycastTarget = true;
        SnowUIFactory.SetRect(
            handle.rectTransform, new Vector2(0f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(32f, 32f), Vector2.zero);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VolumePreference, AudioListener.volume));
        slider.onValueChanged.AddListener(ApplyMasterVolume);
    }

    private void CreateFullscreenToggle(Transform parent)
    {
        GameObject toggleObject = new GameObject(
            "FullscreenToggle",
            typeof(RectTransform),
            typeof(UnityEngine.UI.Toggle));
        toggleObject.layer = parent.gameObject.layer;
        toggleObject.transform.SetParent(parent, false);
        SnowUIFactory.SetRect(
            toggleObject.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(500f, 58f), new Vector2(0f, -8f));

        UnityEngine.UI.Image background = SnowUIFactory.CreateImage(toggleObject.transform, "ToggleBackground");
        SnowUIFactory.SetStretch(background.rectTransform);
        background.color = new Color(0.11f, 0.25f, 0.34f, 0.65f);
        background.raycastTarget = true;

        UnityEngine.UI.Image checkmark = SnowUIFactory.CreateImage(toggleObject.transform, "Checkmark");
        SnowUIFactory.SetRect(
            checkmark.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(28f, 28f), new Vector2(17f, 0f));
        checkmark.color = SnowUIFactory.Cyan;

        SnowUIFactory.CreateText(
            toggleObject.transform, "FullscreenLabel", "FULLSCREEN",
            22f, SnowUIFactory.Ice, TextAlignmentOptions.Left,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(330f, 40f), new Vector2(67f, 0f), FontStyles.Bold);

        UnityEngine.UI.Toggle toggle = toggleObject.GetComponent<UnityEngine.UI.Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(FullscreenPreference, Screen.fullScreen ? 1 : 0) == 1);
        toggle.onValueChanged.AddListener(value => ApplyFullscreen(value, true));
    }

    private void PauseGame()
    {
        isPaused = true;
        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        overlayBackdrop.SetActive(true);
        ShowPausePage();
    }

    private void ResumeGame()
    {
        if (isPaused)
            Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        isPaused = false;
        PlayerPrefs.Save();
        if (overlayBackdrop != null)
            overlayBackdrop.SetActive(false);
        if (pausePage != null)
            pausePage.SetActive(false);
        if (settingsPage != null)
            settingsPage.SetActive(false);
    }

    private void ShowSettingsPage()
    {
        pausePage.SetActive(false);
        settingsPage.SetActive(true);
        SelectFirstButton(settingsPage.transform);
    }

    private void ShowPausePage()
    {
        PlayerPrefs.Save();
        settingsPage.SetActive(false);
        pausePage.SetActive(true);
        SelectFirstButton(pausePage.transform);
    }

    private static void SelectFirstButton(Transform root)
    {
        if (EventSystem.current == null || root == null)
            return;

        UnityEngine.UI.Selectable selectable = root.GetComponentInChildren<UnityEngine.UI.Selectable>();
        if (selectable != null)
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }

    private void ApplyMasterVolume(float value)
    {
        float volume = Mathf.Clamp01(value);
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumePreference, volume);
        if (volumeValue != null)
            volumeValue.text = Mathf.RoundToInt(volume * 100f) + "%";
    }

    private static void ApplyFullscreen(bool fullscreen, bool savePreference)
    {
        Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreen = fullscreen;
        if (!savePreference)
            return;

        PlayerPrefs.SetInt(FullscreenPreference, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ReturnToTitle()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();
        SceneManager.LoadScene("TitleScene");
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetButtonColors(UnityEngine.UI.Button button, Color color)
    {
        UnityEngine.UI.ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(1f, 0.76f, 0.7f, 1f);
        colors.pressedColor = new Color(0.75f, 0.27f, 0.25f, 1f);
        colors.selectedColor = new Color(1f, 0.69f, 0.62f, 1f);
        button.colors = colors;
    }

    private void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }
}

