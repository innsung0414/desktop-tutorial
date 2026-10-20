using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject restartText;
    public GameObject gameOverPanel;

    private float score;
    private float bestScore;
    private int lastDisplayedSecond = -1;
    private TextMeshProUGUI gameOverScoreText;
    private TextMeshProUGUI gameOverBestText;
    private TextMeshProUGUI newRecordText;
    private TextMeshProUGUI leaderboardPromptText;
    private TextMeshProUGUI recordSaveStatusText;
    private TMP_Text gameOverTitle;
    private TextMeshProUGUI speedValueText;
    private TMP_InputField nicknameInput;
    private Button submitRecordButton;
    private TMP_Text restartHint;
    private readonly Image[] leaderboardRowBackgrounds = new Image[ArcadeLeaderboard.MaxEntries];
    private readonly TextMeshProUGUI[] leaderboardRankTexts = new TextMeshProUGUI[ArcadeLeaderboard.MaxEntries];
    private readonly TextMeshProUGUI[] leaderboardNameTexts = new TextMeshProUGUI[ArcadeLeaderboard.MaxEntries];
    private readonly TextMeshProUGUI[] leaderboardScoreTexts = new TextMeshProUGUI[ArcadeLeaderboard.MaxEntries];
    private List<ArcadeLeaderboard.ScoreEntry> leaderboardEntries;
    private PlayerController playerController;
    private float nextSpeedRefreshTime;
    private int pendingScoreSeconds;
    private int pendingLeaderboardRank;
    private bool isRestarting;
    private bool waitingForNickname;
    private bool scoreWasSaved;

    public bool IsGameOver { get; private set; }

    private void Start()
    {
        Time.timeScale = 1f;
        score = 0f;
        leaderboardEntries = ArcadeLeaderboard.Load();
        bestScore = ArcadeLeaderboard.BestScoreSeconds(leaderboardEntries);
        playerController = FindFirstObjectByType<PlayerController>();

        Canvas canvas = scoreText != null ? scoreText.canvas : FindFirstObjectByType<Canvas>();
        SnowUIFactory.ConfigureCanvas(canvas);
        ConfigureHud(canvas);
        ConfigureGameOverUI(canvas);
        if (GetComponent<PauseMenu>() == null)
            gameObject.AddComponent<PauseMenu>();
        RefreshScoreText(true);

        if (TitleManager.IsAttractMode && canvas != null)
        {
            CanvasGroup hudGroup = canvas.GetComponent<CanvasGroup>();
            if (hudGroup == null)
                hudGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            hudGroup.alpha = 0f;
            hudGroup.interactable = false;
            hudGroup.blocksRaycasts = false;
        }

        if (restartText != null)
            restartText.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (TitleManager.IsAttractMode)
            return;

        if (!IsGameOver)
        {
            score += Time.deltaTime;
            RefreshScoreText(false);
            RefreshSpeedText(false);
            return;
        }

        if (waitingForNickname)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                RestartGame();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
            RestartGame();
    }

    public void GameOver()
    {
        if (IsGameOver || TitleManager.IsAttractMode)
            return;

        IsGameOver = true;
        pendingScoreSeconds = Mathf.Max(0, Mathf.FloorToInt(score));
        leaderboardEntries = ArcadeLeaderboard.Load();
        pendingLeaderboardRank = ArcadeLeaderboard.GetQualifyingRank(pendingScoreSeconds, leaderboardEntries);
        bestScore = ArcadeLeaderboard.BestScoreSeconds(leaderboardEntries);
        waitingForNickname = pendingLeaderboardRank > 0;
        scoreWasSaved = false;

        RefreshScoreText(true);
        RefreshSpeedText(true);
        if (gameOverScoreText != null)
            gameOverScoreText.text = "TIME SURVIVED   " + FormatDuration(score);
        if (gameOverBestText != null)
            gameOverBestText.text = "ARCADE BEST   " + FormatDuration(bestScore);
        if (newRecordText != null)
        {
            newRecordText.text = "NEW ARCADE RECORD!";
            newRecordText.gameObject.SetActive(waitingForNickname && pendingLeaderboardRank == 1);
        }
        if (leaderboardPromptText != null)
        {
            leaderboardPromptText.text = waitingForNickname
                ? "ENTER A NICKNAME TO SAVE AS #" + pendingLeaderboardRank
                : "ONLY THE TOP FIVE RUNS ARE KEPT";
            leaderboardPromptText.color = waitingForNickname ? SnowUIFactory.Cyan : SnowUIFactory.MutedIce;
        }
        if (nicknameInput != null)
        {
            nicknameInput.text = string.Empty;
            nicknameInput.interactable = waitingForNickname;
            nicknameInput.gameObject.SetActive(waitingForNickname);
        }
        if (submitRecordButton != null)
        {
            submitRecordButton.interactable = waitingForNickname;
            submitRecordButton.gameObject.SetActive(waitingForNickname);
        }
        if (recordSaveStatusText != null)
            recordSaveStatusText.text = string.Empty;
        RefreshLeaderboardRows(leaderboardEntries, 0);

        if (restartText != null)
            restartText.SetActive(true);
        SetRestartHint(waitingForNickname
            ? "ENTER SAVES   /   ESC SKIPS"
            : "PRESS R OR CLICK TO RIDE AGAIN");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }

        if (waitingForNickname && nicknameInput != null)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(nicknameInput.gameObject);
            nicknameInput.Select();
            nicknameInput.ActivateInputField();
        }

        Time.timeScale = 0f;
    }

    public void SubmitLeaderboardScore()
    {
        if (!IsGameOver || !waitingForNickname || scoreWasSaved || nicknameInput == null)
            return;

        string nickname = nicknameInput.text.Trim();
        if (nickname.Length == 0)
        {
            if (recordSaveStatusText != null)
            {
                recordSaveStatusText.text = "TYPE A NICKNAME BEFORE SAVING";
                recordSaveStatusText.color = new Color(1f, 0.69f, 0.35f, 1f);
            }
            nicknameInput.ActivateInputField();
            return;
        }

        int savedRank;
        if (!ArcadeLeaderboard.Submit(nickname, pendingScoreSeconds, out savedRank))
        {
            leaderboardEntries = ArcadeLeaderboard.Load();
            waitingForNickname = false;
            pendingLeaderboardRank = -1;
            if (leaderboardPromptText != null)
            {
                leaderboardPromptText.text = "THIS RUN NO LONGER QUALIFIES FOR TOP 5";
                leaderboardPromptText.color = SnowUIFactory.MutedIce;
            }
            if (nicknameInput != null)
                nicknameInput.gameObject.SetActive(false);
            if (submitRecordButton != null)
                submitRecordButton.gameObject.SetActive(false);
            RefreshLeaderboardRows(leaderboardEntries, 0);
            return;
        }

        scoreWasSaved = true;
        waitingForNickname = false;
        leaderboardEntries = ArcadeLeaderboard.Load();
        bestScore = ArcadeLeaderboard.BestScoreSeconds(leaderboardEntries);
        RefreshScoreText(true);
        if (gameOverBestText != null)
            gameOverBestText.text = "ARCADE BEST   " + FormatDuration(bestScore);
        if (leaderboardPromptText != null)
        {
            leaderboardPromptText.text = "SAVED TO LOCAL TOP 5  •  RANK #" + savedRank;
            leaderboardPromptText.color = SnowUIFactory.Cyan;
        }
        if (recordSaveStatusText != null)
            recordSaveStatusText.text = "THANKS, " + nickname.ToUpperInvariant();
        SetRestartHint("PRESS R OR CLICK TO RIDE AGAIN");
        if (nicknameInput != null)
            nicknameInput.interactable = false;
        if (submitRecordButton != null)
            submitRecordButton.interactable = false;
        RefreshLeaderboardRows(leaderboardEntries, savedRank);
    }

    public void RestartGame()
    {
        if (isRestarting)
            return;

        isRestarting = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RefreshScoreText(bool force)
    {
        int displayedSecond = Mathf.FloorToInt(score);
        if (!force && displayedSecond == lastDisplayedSecond)
            return;

        lastDisplayedSecond = displayedSecond;
        if (scoreText != null)
            scoreText.text = "TIME   " + FormatDuration(score);
        if (bestScoreText != null)
            bestScoreText.text = "BEST   " + FormatDuration(bestScore);
    }

    private void ConfigureHud(Canvas canvas)
    {
        if (canvas == null)
            return;

        Transform canvasRoot = canvas.transform;
        if (scoreText != null || bestScoreText != null)
        {
            Image scoreCard = SnowUIFactory.CreatePanel(
                canvasRoot,
                "ScoreCard",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(350f, 126f),
                new Vector2(28f, -28f),
                SnowUIFactory.Navy);
            scoreCard.transform.SetAsFirstSibling();

            if (scoreText != null)
            {
                scoreText.transform.SetParent(scoreCard.transform, false);
                SnowUIFactory.SetRect(
                    scoreText.rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(306f, 55f),
                    new Vector2(25f, -16f));
                StyleText(scoreText, 39f, SnowUIFactory.Ice, TextAlignmentOptions.Left);
            }

            if (bestScoreText != null)
            {
                bestScoreText.transform.SetParent(scoreCard.transform, false);
                SnowUIFactory.SetRect(
                    bestScoreText.rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(306f, 38f),
                    new Vector2(27f, -78f));
                StyleText(bestScoreText, 23f, SnowUIFactory.MutedIce, TextAlignmentOptions.Left);
            }
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        TextMeshProUGUI jumpText = player != null
            ? player.doubleJumpText
            : GameObject.Find("DoubleJumpText")?.GetComponent<TextMeshProUGUI>();
        if (jumpText != null)
        {
            Image jumpCard = SnowUIFactory.CreatePanel(
                canvasRoot,
                "DoubleJumpCard",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(350f, 92f),
                new Vector2(-28f, -28f),
                SnowUIFactory.Navy);

            Image itemIcon = SnowUIFactory.CreateImage(jumpCard.transform, "DoubleJumpIcon");
            itemIcon.sprite = Resources.Load<Sprite>("JumpDashUI/double_jump_item");
            itemIcon.preserveAspect = true;
            SnowUIFactory.SetRect(
                itemIcon.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(58f, 58f),
                new Vector2(20f, -17f));
            if (itemIcon.sprite == null)
                itemIcon.gameObject.SetActive(false);

            jumpText.transform.SetParent(jumpCard.transform, false);
            SnowUIFactory.SetRect(
                jumpText.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(250f, 58f),
                new Vector2(91f, -19f));
            StyleText(jumpText, 24f, SnowUIFactory.Ice, TextAlignmentOptions.Left);
        }

        ConfigureDashHud(canvasRoot);
        ConfigureRideSpeedHud(canvasRoot);
    }

    private void ConfigureRideSpeedHud(Transform canvasRoot)
    {
        if (canvasRoot == null)
            return;

        Image speedCard = SnowUIFactory.CreatePanel(
            canvasRoot,
            "RideSpeedCard",
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(228f, 96f),
            new Vector2(0f, -28f),
            SnowUIFactory.Navy);

        TextMeshProUGUI label = SnowUIFactory.CreateText(
            speedCard.transform,
            "RideSpeedLabel",
            "TRAIL SPEED",
            16f,
            SnowUIFactory.MutedIce,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(204f, 28f),
            new Vector2(0f, -10f),
            FontStyles.Bold);
        label.characterSpacing = 1.3f;

        speedValueText = SnowUIFactory.CreateText(
            speedCard.transform,
            "RideSpeedValue",
            "1.0x",
            36f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(204f, 48f),
            new Vector2(0f, 8f),
            FontStyles.Bold);
        RefreshSpeedText(true);
    }

    private void RefreshSpeedText(bool force)
    {
        if (speedValueText == null || (!force && Time.unscaledTime < nextSpeedRefreshTime))
            return;

        nextSpeedRefreshTime = Time.unscaledTime + 0.1f;
        float speed = playerController != null ? playerController.moveSpeed : 5f;
        speedValueText.text = (speed / 5f).ToString("0.0") + "x";
    }

    private void ConfigureDashHud(Transform canvasRoot)
    {
        DashCooldownUI dashUI = FindFirstObjectByType<DashCooldownUI>();
        if (dashUI == null || dashUI.dashBar == null)
            return;

        Image dashCard = SnowUIFactory.CreatePanel(
            canvasRoot,
            "DashStatusCard",
            Vector2.zero,
            Vector2.zero,
            new Vector2(410f, 106f),
            new Vector2(30f, 30f),
            SnowUIFactory.Navy);

        TextMeshProUGUI status = SnowUIFactory.CreateText(
            dashCard.transform,
            "DashStatusText",
            "DASH READY",
            19f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Left,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(250f, 35f),
            new Vector2(24f, -15f),
            FontStyles.Bold);
        status.characterSpacing = 1f;
        dashUI.statusText = status;

        Image tabKey = SnowUIFactory.CreatePanel(
            dashCard.transform,
            "DashKey",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(70f, 34f),
            new Vector2(-22f, -14f),
            new Color(0.18f, 0.4f, 0.53f, 0.86f));
        SnowUIFactory.CreateText(
            tabKey.transform,
            "DashKeyLabel",
            "TAB",
            17f,
            SnowUIFactory.Ice,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(66f, 30f),
            Vector2.zero,
            FontStyles.Bold);

        Image background = GameObject.Find("DashCooldownBar_Background")?.GetComponent<Image>();
        if (background != null)
        {
            background.transform.SetParent(dashCard.transform, false);
            background.sprite = SnowUIFactory.RoundedSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.11f, 0.22f, 0.32f, 1f);
            background.raycastTarget = false;
            SnowUIFactory.SetRect(
                background.rectTransform,
                Vector2.zero,
                Vector2.zero,
                new Vector2(360f, 17f),
                new Vector2(24f, 21f));
        }

        Image fill = dashUI.dashBar;
        fill.transform.SetParent(dashCard.transform, false);
        fill.sprite = SnowUIFactory.RoundedSprite;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = SnowUIFactory.Cyan;
        fill.raycastTarget = false;
        SnowUIFactory.SetRect(
            fill.rectTransform,
            Vector2.zero,
            Vector2.zero,
            new Vector2(360f, 17f),
            new Vector2(24f, 21f));
    }

    private void ConfigureGameOverUI(Canvas canvas)
    {
        if (gameOverPanel == null)
            return;

        Image overlay = gameOverPanel.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.color = new Color(0.015f, 0.055f, 0.11f, 0.86f);
            overlay.raycastTarget = false;
        }

        Image resultCard = SnowUIFactory.CreatePanel(
            gameOverPanel.transform,
            "GameOverCard",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(1080f, 660f),
            Vector2.zero,
            new Color(0.035f, 0.13f, 0.22f, 0.97f));

        Image resultColumn = SnowUIFactory.CreatePanel(
            resultCard.transform,
            "RunResultColumn",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(480f, 500f),
            new Vector2(-263f, -25f),
            new Color(0.02f, 0.085f, 0.15f, 0.76f));
        Image leaderboardPanel = SnowUIFactory.CreatePanel(
            resultCard.transform,
            "ArcadeLeaderboardPanel",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(480f, 500f),
            new Vector2(263f, -25f),
            new Color(0.02f, 0.085f, 0.15f, 0.76f));

        gameOverTitle = FindText(gameOverPanel.transform, "GameOverText");
        if (gameOverTitle == null)
        {
            gameOverTitle = SnowUIFactory.CreateText(
                resultCard.transform,
                "GameOverText",
                "RUN OVER",
                55f,
                SnowUIFactory.Ice,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(980f, 74f),
                new Vector2(0f, -24f),
                FontStyles.Bold);
        }
        else
        {
            gameOverTitle.transform.SetParent(resultCard.transform, false);
            SnowUIFactory.SetRect(
                gameOverTitle.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(980f, 74f),
                new Vector2(0f, -24f));
            gameOverTitle.text = "RUN OVER";
            StyleText(gameOverTitle, 55f, SnowUIFactory.Ice, TextAlignmentOptions.Center);
        }

        gameOverScoreText = SnowUIFactory.CreateText(
            resultColumn.transform,
            "RunTimeResult",
            "TIME SURVIVED   00:00",
            26f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(430f, 44f),
            new Vector2(0f, 166f),
            FontStyles.Bold);
        gameOverBestText = SnowUIFactory.CreateText(
            resultColumn.transform,
            "RunBestResult",
            "ARCADE BEST   00:00",
            22f,
            SnowUIFactory.Ice,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(430f, 40f),
            new Vector2(0f, 117f),
            FontStyles.Normal);
        newRecordText = SnowUIFactory.CreateText(
            resultColumn.transform,
            "NewRecordNotice",
            "NEW ARCADE RECORD!",
            19f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(430f, 32f),
            new Vector2(0f, 75f),
            FontStyles.Bold);
        newRecordText.gameObject.SetActive(false);

        leaderboardPromptText = SnowUIFactory.CreateText(
            resultColumn.transform,
            "NicknamePrompt",
            "ENTER A NICKNAME TO SAVE",
            16f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(430f, 32f),
            new Vector2(0f, 35f),
            FontStyles.Bold);

        Image nicknameFieldImage = SnowUIFactory.CreatePanel(
            resultColumn.transform,
            "NicknameInputField",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(400f, 56f),
            new Vector2(0f, -17f),
            new Color(0.09f, 0.24f, 0.33f, 1f));
        nicknameFieldImage.raycastTarget = true;
        nicknameInput = nicknameFieldImage.gameObject.AddComponent<TMP_InputField>();
        nicknameInput.targetGraphic = nicknameFieldImage;
        nicknameInput.characterLimit = ArcadeLeaderboard.NicknameLimit;
        nicknameInput.lineType = TMP_InputField.LineType.SingleLine;
        nicknameInput.contentType = TMP_InputField.ContentType.Standard;
        nicknameInput.textViewport = nicknameFieldImage.rectTransform;
        nicknameInput.caretColor = SnowUIFactory.Cyan;
        nicknameInput.selectionColor = new Color(SnowUIFactory.Cyan.r, SnowUIFactory.Cyan.g, SnowUIFactory.Cyan.b, 0.32f);

        TextMeshProUGUI inputText = SnowUIFactory.CreateText(
            nicknameFieldImage.transform,
            "NicknameText",
            string.Empty,
            22f,
            SnowUIFactory.Ice,
            TextAlignmentOptions.Left,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero);
        SnowUIFactory.SetStretch(inputText.rectTransform);
        inputText.rectTransform.offsetMin = new Vector2(18f, 5f);
        inputText.rectTransform.offsetMax = new Vector2(-18f, -5f);
        nicknameInput.textComponent = inputText;

        TextMeshProUGUI inputPlaceholder = SnowUIFactory.CreateText(
            nicknameFieldImage.transform,
            "NicknamePlaceholder",
            "TYPE YOUR NICKNAME",
            18f,
            SnowUIFactory.MutedIce,
            TextAlignmentOptions.Left,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.zero);
        SnowUIFactory.SetStretch(inputPlaceholder.rectTransform);
        inputPlaceholder.rectTransform.offsetMin = new Vector2(18f, 5f);
        inputPlaceholder.rectTransform.offsetMax = new Vector2(-18f, -5f);
        nicknameInput.placeholder = inputPlaceholder;
        nicknameInput.onSubmit.AddListener(delegate { SubmitLeaderboardScore(); });

        submitRecordButton = SnowUIFactory.CreateButton(
            resultColumn.transform,
            "SaveArcadeScoreButton",
            "SAVE ARCADE SCORE",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(400f, 58f),
            new Vector2(0f, -91f),
            SubmitLeaderboardScore);
        submitRecordButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 21f;

        SnowUIFactory.CreateButton(
            resultColumn.transform,
            "RestartButton",
            "SKIP  /  RIDE AGAIN",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(400f, 58f),
            new Vector2(0f, -171f),
            RestartGame).GetComponent<Image>().color = new Color(0.2f, 0.39f, 0.48f, 1f);

        recordSaveStatusText = SnowUIFactory.CreateText(
            resultColumn.transform,
            "RecordSaveStatus",
            string.Empty,
            15f,
            SnowUIFactory.MutedIce,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(430f, 25f),
            new Vector2(0f, 31f),
            FontStyles.Bold);

        TextMeshProUGUI boardHeading = SnowUIFactory.CreateText(
            leaderboardPanel.transform,
            "GameOverTopFiveHeading",
            "ARCADE TOP 5",
            22f,
            SnowUIFactory.Cyan,
            TextAlignmentOptions.Left,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(430f, 38f),
            new Vector2(24f, -22f),
            FontStyles.Bold);
        boardHeading.characterSpacing = 1f;
        SnowUIFactory.CreateText(
            leaderboardPanel.transform,
            "GameOverTopFiveColumns",
            "RANK                       RIDER                         TIME",
            14f,
            SnowUIFactory.MutedIce,
            TextAlignmentOptions.Left,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(430f, 26f),
            new Vector2(24f, -72f),
            FontStyles.Bold);

        for (int i = 0; i < ArcadeLeaderboard.MaxEntries; i++)
        {
            float y = 126f - i * 57f;
            Image row = SnowUIFactory.CreatePanel(
                leaderboardPanel.transform,
                "TopFiveRow_" + (i + 1),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(430f, 48f),
                new Vector2(0f, y),
                new Color(0.04f, 0.15f, 0.23f, 0.72f));
            leaderboardRowBackgrounds[i] = row;
            leaderboardRankTexts[i] = SnowUIFactory.CreateText(
                row.transform,
                "Rank",
                (i + 1).ToString("00"),
                20f,
                SnowUIFactory.Cyan,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(54f, 32f),
                new Vector2(17f, 0f),
                FontStyles.Bold);
            leaderboardNameTexts[i] = SnowUIFactory.CreateText(
                row.transform,
                "Nickname",
                "---",
                19f,
                SnowUIFactory.Ice,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(245f, 32f),
                new Vector2(80f, 0f),
                FontStyles.Bold);
            leaderboardScoreTexts[i] = SnowUIFactory.CreateText(
                row.transform,
                "Score",
                "--:--",
                19f,
                SnowUIFactory.Ice,
                TextAlignmentOptions.Right,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(84f, 32f),
                new Vector2(-17f, 0f),
                FontStyles.Bold);
        }

        RefreshLeaderboardRows(ArcadeLeaderboard.Load(), 0);

        if (restartText != null)
        {
            restartText.transform.SetParent(resultColumn.transform, false);
            restartHint = restartText.GetComponent<TMP_Text>() ?? restartText.GetComponentInChildren<TMP_Text>(true);
            if (restartHint != null)
            {
                SnowUIFactory.SetRect(
                    restartHint.rectTransform,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(430f, 22f),
                    new Vector2(0f, 7f));
                restartHint.text = "ENTER SAVES   /   ESC SKIPS";
                StyleText(restartHint, 14f, SnowUIFactory.MutedIce, TextAlignmentOptions.Center);
            }
        }

        if (canvas != null)
            gameOverPanel.transform.SetAsLastSibling();
    }

    private void RefreshLeaderboardRows(IList<ArcadeLeaderboard.ScoreEntry> entries, int highlightedRank)
    {
        for (int i = 0; i < ArcadeLeaderboard.MaxEntries; i++)
        {
            bool hasEntry = entries != null && i < entries.Count;
            if (leaderboardRankTexts[i] != null)
                leaderboardRankTexts[i].text = (i + 1).ToString("00");
            if (leaderboardNameTexts[i] != null)
                leaderboardNameTexts[i].text = hasEntry ? entries[i].nickname : "---";
            if (leaderboardScoreTexts[i] != null)
                leaderboardScoreTexts[i].text = hasEntry ? FormatDuration(entries[i].scoreSeconds) : "--:--";
            if (leaderboardRowBackgrounds[i] != null)
            {
                bool highlight = highlightedRank == i + 1;
                leaderboardRowBackgrounds[i].color = highlight
                    ? new Color(SnowUIFactory.Cyan.r, SnowUIFactory.Cyan.g, SnowUIFactory.Cyan.b, 0.22f)
                    : new Color(0.04f, 0.15f, 0.23f, i % 2 == 0 ? 0.78f : 0.5f);
            }
            if (leaderboardRankTexts[i] != null)
                leaderboardRankTexts[i].color = highlightedRank == i + 1 ? SnowUIFactory.Cyan : SnowUIFactory.MutedIce;
            if (leaderboardNameTexts[i] != null)
                leaderboardNameTexts[i].color = hasEntry ? SnowUIFactory.Ice : SnowUIFactory.MutedIce;
            if (leaderboardScoreTexts[i] != null)
                leaderboardScoreTexts[i].color = hasEntry ? SnowUIFactory.Ice : SnowUIFactory.MutedIce;
        }
    }

    private void SetRestartHint(string message)
    {
        if (restartHint != null)
            restartHint.text = message;
    }

    private static TMP_Text FindText(Transform root, string objectName)
    {
        if (root == null)
            return null;

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].gameObject.name == objectName)
                return texts[i];
        }

        return null;
    }

    private static void StyleText(TMP_Text text, float size, Color color, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        text.font = SnowUIFactory.Font;
        text.fontSize = size;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }

    private static string FormatDuration(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return string.Format("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }
}
