using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 메뉴만 담당합니다. 낚시 규칙은 FishingGameController에 그대로 두고,
/// 메뉴가 열려 있는 동안 입력과 게임 시간을 멈춥니다.
/// </summary>
[DefaultExecutionOrder(-200)]
public class DeepBlueMenus : MonoBehaviour
{
    [SerializeField] private Texture2D titleBackground;
    private enum Page { Playing, Title, Pause, Guide, Settings, Journal, Death, Confirm }
    private readonly Dictionary<Page, GameObject> pages = new Dictionary<Page, GameObject>();
    private readonly Dictionary<Page, GameObject> firstButtons = new Dictionary<Page, GameObject>();
    private static readonly Color Ink = new Color(0.025f, 0.07f, 0.09f, 0.98f);
    private static readonly Color Muted = new Color(0.66f, 0.80f, 0.81f);
    private static readonly Color Teal = new Color(0.38f, 0.88f, 0.83f);
    private static readonly Color Gold = new Color(1f, 0.8f, 0.44f);
    private static bool startAfterReload;
    public static DeepBlueMenus Instance { get; private set; }
    public static bool IsMenuOpen => Instance != null && Instance.page != Page.Playing;
    public static bool BlocksGameplay => Instance != null &&
        (IsMenuOpen || Time.unscaledTime < Instance.resumeAfter || Instance.waitForMouseRelease);

    private Page page = Page.Title;
    private Page returnPage = Page.Title;
    private Page confirmReturn;
    private UnityAction confirmedAction;
    private GameObject overlay;
    private GameObject background;
    private GameObject journalGrid;
    private UnityEngine.UI.Text journalSummary;
    private const int JournalPageSize = 6;
    private int journalPage;
    private UnityEngine.UI.Text journalPageLabel;
    private UnityEngine.UI.Button journalPrevious;
    private UnityEngine.UI.Button journalNext;
    private UnityEngine.UI.Text titleSummary;
    private UnityEngine.UI.Text confirmMessage;
    private UnityEngine.UI.Text volumeValue;
    private UnityEngine.UI.Text sensitivityValue;
    private Font font;
    private bool ownsFont;
    private FishingGameController fishing;
    private DrowningController drowning;
    private FirstPersonLook look;
    private float resumeAfter;
    private bool waitForMouseRelease;
    private bool sessionStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        startAfterReload = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { enabled = false; return; }
        Instance = this;
        fishing = GetComponent<FishingGameController>();
        drowning = FindFirstObjectByType<DrowningController>();
        look = FindFirstObjectByType<FirstPersonLook>();
        font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 24);
        ownsFont = font != null;
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        AudioListener.volume = DeepBlueProfile.Volume;
        BuildMenus();
        Show(Page.Title);
    }

    private void Start()
    {
        if (startAfterReload)
        {
            startAfterReload = false;
            BeginFishing();
        }
    }

    private void Update()
    {
        if (waitForMouseRelease && (Mouse.current == null || !Mouse.current.leftButton.isPressed))
            waitForMouseRelease = false;
        if (page == Page.Playing && drowning != null && drowning.IsDead) Show(Page.Death);
        if (Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (page == Page.Playing) Show(Page.Pause);
            else if (page == Page.Pause) ResumeFishing();
            else if (page == Page.Confirm) Show(confirmReturn);
            else if (page == Page.Guide || page == Page.Settings || page == Page.Journal) Back();
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (page == Page.Playing) OpenJournal(Page.Playing);
            else if (page == Page.Journal) Back();
        }
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && sessionStarted && page == Page.Playing) Show(Page.Pause);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && sessionStarted && page == Page.Playing) Show(Page.Pause);
        if (paused) DeepBlueProfile.Save();
    }

    private void Show(Page next)
    {
        if (page == Page.Settings) DeepBlueProfile.Save();
        page = next;
        bool inMenu = next != Page.Playing;
        Time.timeScale = inMenu ? 0f : 1f;
        AudioListener.pause = inMenu;
        overlay.SetActive(inMenu);
        background.SetActive(!sessionStarted);
        foreach (var entry in pages) entry.Value.SetActive(entry.Key == next);
        if (look != null) look.SetMenuCursorLocked(!inMenu);
        else { Cursor.lockState = inMenu ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = inMenu; }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstButtons.TryGetValue(next, out var first) ? first : null);
        if (next == Page.Title) titleSummary.text = CollectionSummary() + "\n낚시 기록 / FIELD NOTES";
    }

    private void BeginFishing()
    {
        sessionStarted = true;
        ResumeFishing();
    }

    private void ResumeFishing()
    {
        // 버튼을 누른 같은 프레임의 클릭/키가 캐스팅이나 챔질로 전달되지 않게 합니다.
        resumeAfter = Time.unscaledTime + 0.2f;
        waitForMouseRelease = true;
        Show(Page.Playing);
    }

    private void OpenSubpage(Page next)
    {
        returnPage = page;
        Show(next);
    }

    private void Back()
    {
        if (returnPage == Page.Playing) ResumeFishing();
        else Show(returnPage);
    }

    private void Ask(string message, UnityAction action)
    {
        confirmReturn = page;
        confirmedAction = action;
        confirmMessage.text = message;
        Show(Page.Confirm);
    }

    private void Reload(bool begin)
    {
        DeepBlueProfile.Save();
        startAfterReload = begin;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Quit()
    {
        DeepBlueProfile.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        DeepBlueProfile.Save();
        Instance = null;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (ownsFont && font != null) Destroy(font);
        font = null;
        ownsFont = false;
    }

    private void BuildMenus()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("Deep Blue EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }
        overlay = new GameObject("Deep Blue Menu Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = overlay.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 1f;

        background = Rect("Title Artwork", overlay.transform, Vector2.zero, Vector2.zero).gameObject;
        Stretch(background.GetComponent<RectTransform>());
        var artwork = background.AddComponent<UnityEngine.UI.RawImage>();
        artwork.texture = titleBackground;
        artwork.color = titleBackground != null ? Color.white : Ink;
        artwork.raycastTarget = false;
        if (titleBackground != null)
        {
            var fitter = background.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = titleBackground.width / (float)titleBackground.height;
        }
        var dim = Panel("Menu Dim", overlay.transform, new Color(0.005f, 0.025f, 0.035f, 0.38f), Vector2.zero, Vector2.zero);
        Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        BuildTitle();
        BuildPause();
        BuildGuide();
        BuildSettings();
        BuildJournal();
        BuildDeath();
        BuildConfirm();
    }

    private void BuildTitle()
    {
        var root = Rect("Title Screen", overlay.transform, Vector2.zero, Vector2.zero);
        Stretch(root);
        pages.Add(Page.Title, root.gameObject);
        Label("Series", root, "A QUIET MOMENT BY THE WATER", 14, Teal, new Vector2(72, 58), new Vector2(620, 30));
        Label("Game Title", root, "DEEP BLUE", 80, Color.white, new Vector2(66, 99), new Vector2(700, 110), true);
        Label("Tagline", root, "물결을 따라, 나만의 한 마리를 만나다.", 21, Muted, new Vector2(72, 218), new Vector2(600, 40));
        Panel("Title Rule", root, Teal, new Vector2(72, 282), new Vector2(48, 3));
        firstButtons[Page.Title] = Button("Start Fishing", root, "낚시 시작   →", new Vector2(72, 316), new Vector2(320, 56), BeginFishing, true).gameObject;
        Button("Open Guide", root, "조작법", new Vector2(72, 384), new Vector2(320, 48), () => OpenSubpage(Page.Guide));
        Button("Open Journal", root, "물고기 도감", new Vector2(72, 444), new Vector2(320, 48), () => OpenJournal(Page.Title));
        Button("Open Settings", root, "설정", new Vector2(72, 504), new Vector2(320, 48), () => OpenSubpage(Page.Settings));
        Button("Quit Game", root, "게임 종료", new Vector2(72, 564), new Vector2(320, 48), () => Ask("게임을 종료할까요?\n도감과 설정은 자동 저장됩니다.", Quit));
        Label("Title Footer", root, "혼자 즐기는 호숫가 낚시  /  DEEP BLUE", 13, Muted, new Vector2(72, 662), new Vector2(600, 24));
        titleSummary = Label("Title Collection", root, "", 17, Color.white, new Vector2(-54, -52), new Vector2(320, 60));
        var rect = titleSummary.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-54, -52);
        titleSummary.alignment = TextAnchor.UpperRight;
    }

    private RectTransform Card(Page id, string heading, string subtitle, float width = 640, float height = 580)
    {
        var panel = Panel(id + " Screen", overlay.transform, Ink, Vector2.zero, new Vector2(width, height));
        var rect = panel.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        pages.Add(id, panel.gameObject);
        Panel("Accent", rect, Teal, new Vector2(32, 30), new Vector2(40, 3));
        Label("Heading", rect, heading, 34, Color.white, new Vector2(32, 48), new Vector2(width - 64, 52), true);
        Label("Description", rect, subtitle, 16, Muted, new Vector2(32, 108), new Vector2(width - 64, 48));
        return rect;
    }

    private void BuildPause()
    {
        var root = Card(Page.Pause, "잠시 쉬어가기", "물결도, 입질도 잠시 기다립니다.");
        firstButtons[Page.Pause] = Button("Resume Fishing", root, "낚시 계속   →", new Vector2(32, 184), new Vector2(576, 54), ResumeFishing, true).gameObject;
        Button("Pause Journal", root, "물고기 도감", new Vector2(32, 252), new Vector2(576, 48), () => OpenJournal(Page.Pause));
        Button("Pause Settings", root, "설정", new Vector2(32, 312), new Vector2(576, 48), () => OpenSubpage(Page.Settings));
        Button("Pause Guide", root, "조작법", new Vector2(32, 372), new Vector2(576, 48), () => OpenSubpage(Page.Guide));
        Button("Return to Title", root, "타이틀로 돌아가기", new Vector2(32, 448), new Vector2(576, 48), () => Ask("타이틀로 돌아갈까요?\n진행 중인 낚시는 끝나고, 도감과 설정은 유지됩니다.", () => Reload(false)));
        Label("Pause Hint", root, "ESC  낚시 계속", 14, Muted, new Vector2(32, 522), new Vector2(576, 24));
    }

    private void BuildGuide()
    {
        var root = Card(Page.Guide, "처음 하는 낚시", "서두르지 말고, 줄의 힘을 느껴 보세요.", 880, 620);
        string[] names = { "01   물가 둘러보기", "02   찌 던지기", "03   입질에 반응하기", "04   감고, 쉬고", "05   기록 모으기", "06   물을 조심하기" };
        string[] details = { "WASD 이동 · 마우스 시점\nESC 메뉴 · TAB 도감", "왼쪽 버튼을 길게 누르면 멀리.\n버튼을 놓으면 찌를 던집니다.", "입질 알림 후 5초 안에\nSpace 또는 왼쪽 버튼으로 챔질.", "왼쪽 버튼을 누르면 릴링.\n장력이 높아지면 놓아 주세요.", "성공하면 도감에 자동 기록.\n결과에서 왼쪽 클릭으로 재도전.", "물속에서 10초면 익사합니다.\n카운트다운이 끝나기 전에 탈출!" };
        for (int i = 0; i < names.Length; i++)
        {
            float x = 32 + i % 2 * 420, y = 174 + i / 2 * 110;
            Label("Step " + i, root, names[i], 19, Teal, new Vector2(x, y), new Vector2(390, 30), true);
            Label("Instruction " + i, root, details[i], 17, Color.white, new Vector2(x, y + 36), new Vector2(390, 60));
        }
        firstButtons[Page.Guide] = Button("Guide Back", root, "돌아가기   /   ESC", new Vector2(32, 536), new Vector2(816, 50), Back).gameObject;
    }

    private void BuildSettings()
    {
        var root = Card(Page.Settings, "나에게 맞게", "변경 사항은 자동 저장됩니다.", 720, 550);
        Label("Volume Label", root, "전체 음량", 21, Color.white, new Vector2(32, 174), new Vector2(360, 36));
        volumeValue = Label("Volume Value", root, "", 19, Teal, new Vector2(582, 174), new Vector2(100, 36));
        var volume = Slider("Master Volume", root, new Vector2(32, 222), new Vector2(656, 34), 0f, 1f, DeepBlueProfile.Volume);
        volume.onValueChanged.AddListener(value => { DeepBlueProfile.SetVolume(value); UpdateSettingLabels(); });
        Label("Sensitivity Label", root, "마우스 감도", 21, Color.white, new Vector2(32, 298), new Vector2(360, 36));
        sensitivityValue = Label("Sensitivity Value", root, "", 19, Teal, new Vector2(582, 298), new Vector2(100, 36));
        var sensitivity = Slider("Mouse Sensitivity", root, new Vector2(32, 346), new Vector2(656, 34), 0.05f, 1f, DeepBlueProfile.Sensitivity);
        sensitivity.onValueChanged.AddListener(value => { DeepBlueProfile.SetSensitivity(value); UpdateSettingLabels(); });
        Label("Settings Hint", root, "낮은 감도는 시점을 천천히 움직입니다.", 15, Muted, new Vector2(32, 398), new Vector2(656, 28));
        firstButtons[Page.Settings] = volume.gameObject;
        Button("Settings Back", root, "저장하고 돌아가기   /   ESC", new Vector2(32, 466), new Vector2(656, 50), Back);
        UpdateSettingLabels();
    }

    private void UpdateSettingLabels()
    {
        volumeValue.text = Mathf.RoundToInt(DeepBlueProfile.Volume * 100) + "%";
        sensitivityValue.text = DeepBlueProfile.Sensitivity.ToString("0.00");
    }

    private void BuildJournal()
    {
        var root = Card(Page.Journal, "물고기 도감", "호수에서 만날 수 있는 물고기들을 모아 보세요.", 900, 700);
        journalSummary = Label("Collection Progress", root, "", 17, Gold, new Vector2(32, 151), new Vector2(836, 30));
        journalGrid = Rect("Fish Cards", root, new Vector2(32, 198), new Vector2(836, 350)).gameObject;
        var grid = journalGrid.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(268, 166);
        grid.spacing = new Vector2(16, 16);
        grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        journalPrevious = Button("Journal Previous", root, "← 이전", new Vector2(32, 560), new Vector2(200, 42), () => MoveJournalPage(-1));
        journalPageLabel = Label("Journal Page", root, "", 17, Muted, new Vector2(250, 560), new Vector2(400, 42));
        journalPageLabel.alignment = TextAnchor.MiddleCenter;
        journalNext = Button("Journal Next", root, "다음 →", new Vector2(668, 560), new Vector2(200, 42), () => MoveJournalPage(1));
        firstButtons[Page.Journal] = Button("Journal Back", root, "돌아가기   /   ESC", new Vector2(32, 614), new Vector2(836, 50), Back).gameObject;
    }

    private string CollectionSummary()
    {
        int discovered = 0, total = 0;
        for (int i = 0; fishing != null && i < fishing.FishSpeciesCount; i++)
        {
            int count = DeepBlueProfile.CatchCount(fishing.FishSpeciesIcon(i));
            if (count > 0) discovered++;
            total += count;
        }
        return $"발견 {discovered} / { (fishing != null ? fishing.FishSpeciesCount : 0) }종   ·   총 {total}마리";
    }

    private void OpenJournal(Page back)
    {
        returnPage = back;
        // 여섯 카드를 재사용합니다. 어종이 늘어도 화면 밖으로 넘치지 않습니다.
        if (journalGrid.transform.childCount == 0 && fishing != null)
        {
            for (int i = 0; i < JournalPageSize; i++)
            {
                var card = Panel("Fish Card " + i, journalGrid.transform, new Color(0.07f, 0.15f, 0.18f), Vector2.zero, new Vector2(268, 166));
                var fish = Panel("Fish Artwork", card.transform, Color.white, new Vector2(14, 8), new Vector2(240, 90));
                fish.preserveAspect = true;
                Label("Fish Name", card.transform, "", 18, Color.white, new Vector2(14, 100), new Vector2(240, 28), true);
                Label("Fish Count", card.transform, "", 14, Muted, new Vector2(14, 132), new Vector2(240, 24));
            }
        }
        RefreshJournalPage();
        Show(Page.Journal);
    }

    private void MoveJournalPage(int direction)
    {
        journalPage += direction;
        RefreshJournalPage();
    }

    private void RefreshJournalPage()
    {
        int speciesCount = fishing != null ? fishing.FishSpeciesCount : 0;
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(speciesCount / (float)JournalPageSize));
        journalPage = Mathf.Clamp(journalPage, 0, pageCount - 1);
        for (int slot = 0; slot < journalGrid.transform.childCount; slot++)
        {
            int species = journalPage * JournalPageSize + slot;
            Transform card = journalGrid.transform.GetChild(slot);
            card.gameObject.SetActive(species < speciesCount);
            if (species >= speciesCount) continue;
            Sprite icon = fishing.FishSpeciesIcon(species);
            int count = DeepBlueProfile.CatchCount(icon);
            var artwork = card.Find("Fish Artwork").GetComponent<UnityEngine.UI.Image>();
            artwork.sprite = icon;
            artwork.color = count > 0 ? Color.white : new Color(0.4f, 0.53f, 0.56f, 0.6f);
            card.Find("Fish Name").GetComponent<UnityEngine.UI.Text>().text = fishing.FishSpeciesName(species);
            var countLabel = card.Find("Fish Count").GetComponent<UnityEngine.UI.Text>();
            countLabel.text = count > 0 ? $"발견 완료   ·   {count}마리" : "아직 만나지 못했어요";
            countLabel.color = count > 0 ? Teal : Muted;
        }
        journalSummary.text = CollectionSummary() + "   /   획득할 때마다 자동 저장";
        journalPageLabel.text = $"{journalPage + 1} / {pageCount} 페이지";
        journalPrevious.interactable = journalPage > 0;
        journalNext.interactable = journalPage < pageCount - 1;
    }

    private void BuildDeath()
    {
        var root = Card(Page.Death, "물가로 돌아갈 시간", "물속에 너무 오래 머물렀습니다.\n낚은 물고기 기록은 안전하게 저장되어 있어요.", 700, 430);
        firstButtons[Page.Death] = Button("Retry from Shore", root, "물가에서 다시 시작   →", new Vector2(32, 210), new Vector2(636, 56), () => Reload(true), true).gameObject;
        Button("Death to Title", root, "타이틀로 돌아가기", new Vector2(32, 284), new Vector2(636, 50), () => Reload(false));
        Label("Death Hint", root, "다음에는 카운트다운이 끝나기 전에 물 밖으로 나오세요.", 15, Muted, new Vector2(32, 362), new Vector2(636, 30));
    }

    private void BuildConfirm()
    {
        var root = Card(Page.Confirm, "잠깐만요", "", 680, 380);
        confirmMessage = Label("Confirmation Message", root, "", 20, Color.white, new Vector2(32, 125), new Vector2(616, 90));
        firstButtons[Page.Confirm] = Button("Cancel", root, "취소", new Vector2(32, 270), new Vector2(296, 54), () => Show(confirmReturn), true).gameObject;
        Button("Confirm", root, "확인", new Vector2(352, 270), new Vector2(296, 54), () => confirmedAction?.Invoke());
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 topLeft, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        var rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(topLeft.x, -topLeft.y);
        rect.sizeDelta = size;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static UnityEngine.UI.Image Panel(string name, Transform parent, Color color, Vector2 position, Vector2 size)
    {
        var image = Rect(name, parent, position, size).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color; image.raycastTarget = false;
        return image;
    }

    private UnityEngine.UI.Text Label(string name, Transform parent, string value, int size, Color color, Vector2 position, Vector2 bounds, bool bold = false)
    {
        var text = Rect(name, parent, position, bounds).gameObject.AddComponent<UnityEngine.UI.Text>();
        text.font = font; text.fontSize = size; text.text = value; text.color = color;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private UnityEngine.UI.Button Button(string name, Transform parent, string caption, Vector2 position, Vector2 size, UnityAction action, bool primary = false)
    {
        var image = Panel(name, parent, primary ? Teal : new Color(0.035f, 0.10f, 0.13f, 0.92f), position, size);
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.72f, 1f, 0.96f);
        colors.selectedColor = new Color(0.72f, 1f, 0.96f);
        colors.pressedColor = new Color(0.48f, 0.73f, 0.7f);
        button.colors = colors;
        button.onClick.AddListener(action);
        Label("Label", image.transform, caption, 20, primary ? Ink : Color.white, new Vector2(20, 0), new Vector2(size.x - 40, size.y), primary);
        return button;
    }

    private UnityEngine.UI.Slider Slider(string name, Transform parent, Vector2 position, Vector2 size, float min, float max, float initial)
    {
        var root = Rect(name, parent, position, size);
        var hitArea = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        hitArea.color = new Color(0.06f, 0.15f, 0.18f); hitArea.raycastTarget = true;
        var fillArea = Rect("Fill Area", root, new Vector2(0, 12), new Vector2(size.x, 10));
        var fill = Panel("Fill", fillArea, Teal, Vector2.zero, Vector2.zero);
        Stretch(fill.rectTransform);
        var handleArea = Rect("Handle Area", root, Vector2.zero, Vector2.zero);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(10, 0); handleArea.offsetMax = new Vector2(-10, 0);
        var handle = Panel("Handle", handleArea, Color.white, Vector2.zero, new Vector2(20, 0));
        handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        handle.raycastTarget = true;
        var slider = root.gameObject.AddComponent<UnityEngine.UI.Slider>();
        slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
        slider.minValue = min; slider.maxValue = max; slider.value = initial;
        return slider;
    }
}
