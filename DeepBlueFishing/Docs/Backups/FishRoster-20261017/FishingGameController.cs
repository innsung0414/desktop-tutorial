using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// 작은 낚시 코어 루프를 담당합니다.
/// 상태마다 받을 입력을 나눠서 초보자도 흐름을 따라가기 쉽게 구성했습니다.
/// </summary>
public class FishingGameController : MonoBehaviour
{
    [System.Serializable]
    private struct FishReward
    {
        public string displayName;
        public Sprite icon;
        [Min(0f)] public float spawnWeight;
    }

    private enum FishingState
    {
        Ready,
        Charging,
        Casting,
        WaitingForBite,
        Bite,
        Reeling,
        Caught,
        Failed
    }

    [Header("장면 연결")]
    [SerializeField] private Transform viewCamera;
    [SerializeField] private Transform rodTip;
    [SerializeField] private Transform bobber;
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private Sprite catchFishIcon;
    [SerializeField] private FishReward[] fishRewards;

    [Header("게임 규칙 - Inspector에서 조정")]
    [SerializeField, Min(0.1f)] private float maxCastChargeSeconds = 2f;
    [SerializeField, Min(0f)] private float minimumBiteDelaySeconds = 3f;
    [SerializeField, Min(0f)] private float maximumBiteDelaySeconds = 8f;
    [SerializeField, Min(0.1f)] private float hookWindowSeconds = 5f;
    [SerializeField, Min(0f)] private float reelProgressPerSecond = 0.25f;
    [SerializeField, Min(0f)] private float tensionGainPerSecond = 0.35f;
    [SerializeField, Min(0f)] private float tensionLossPerSecond = 0.25f;
    [SerializeField] private float waterLevel = 0.05f;
    [SerializeField] private Vector2 waterBoundsX = new Vector2(-24f, 24f);
    [SerializeField] private Vector2 waterBoundsZ = new Vector2(13f, 42f);

    private FishingState state = FishingState.Ready;
    private float castChargeSeconds;
    private float stateTimer;
    private float reelProgress;
    private float lineTension;
    private Vector3 bobberHomePosition;
    private bool hasBobberPosition;
    private FishReward currentFishReward;
    private bool hasCurrentFishReward;
    private string resultMessage;
    private Collider waterVolume;
    private enum FishEffort { Calm, Warning, Surge }
    private FishEffort fishEffort;
    private float effortSeconds;
    private float fishWeightKg;
    private float fishPressure = 1f;
    private float resultDismissAfter;
    private bool firstDiscovery;
    private bool newWeightRecord;
    private string completedGoal;
    private Transform rodRoot;
    private Transform rodShaft;
    private Vector3 rodShaftBaseScale;
    private Vector3 rodShaftBasePosition;
    private Vector3 rodTipBasePosition;
    private Renderer[] bobberRenderers;
    private MaterialPropertyBlock gearPropertyBlock;
    private DeepBlueFishShowcase fishShowcase;

    public bool IsCharging => state == FishingState.Charging;
    public bool IsWaitingForBite => state == FishingState.WaitingForBite;
    public float CastChargeNormalized => maxCastChargeSeconds <= 0f ? 0f : castChargeSeconds / maxCastChargeSeconds;
    public bool IsReeling => state == FishingState.Reeling;
    public float ReelProgress => reelProgress;
    public float LineTension => lineTension;
    public bool IsBiteActive => state == FishingState.Bite;
    public float BiteSecondsRemaining => IsBiteActive ? stateTimer : 0f;
    public bool IsResultVisible => state == FishingState.Caught || state == FishingState.Failed;
    public bool DidCatchFish => state == FishingState.Caught;
    public string PromptText => GetPrompt();
    public string ResultTitle => !DidCatchFish ? "낚시 실패" : firstDiscovery ? "새 물고기 발견!" : newWeightRecord ? "개인 최고 기록!" : "낚시 성공";
    public string ResultMessage => string.IsNullOrEmpty(resultMessage) ? "왼쪽 버튼을 눌러 다시 낚시하세요." : resultMessage;
    public string ResultFishName => hasCurrentFishReward && !string.IsNullOrWhiteSpace(currentFishReward.displayName)
        ? currentFishReward.displayName
        : "무지개송어";
    public Sprite ResultFishIcon => hasCurrentFishReward ? currentFishReward.icon : catchFishIcon;
    public int FishSpeciesCount => fishRewards != null ? fishRewards.Length : 0;
    public string FishSpeciesName(int index) => fishRewards[index].displayName;
    public Sprite FishSpeciesIcon(int index) => fishRewards[index].icon;
    public bool IsFishSurging => IsReeling && fishEffort == FishEffort.Surge;
    public bool IsSurgeWarning => IsReeling && fishEffort == FishEffort.Warning;
    public float ResultWeightKg => DidCatchFish ? fishWeightKg : 0f;
    public bool IsFirstDiscovery => DidCatchFish && firstDiscovery;
    public bool IsNewWeightRecord => DidCatchFish && newWeightRecord;
    public string CompletedGoal => completedGoal;
    public bool CanDismissResult => IsResultVisible && Time.time >= resultDismissAfter;
    public string FightHint => lineTension >= 0.7f ? "장력 위험! 버튼을 놓아 줄을 느슨하게 하세요."
        : IsSurgeWarning ? "곧 강하게 저항합니다! 감기를 멈출 준비를 하세요."
        : IsFishSurging ? "물고기가 힘쓰는 중 · 버튼을 놓고 쉬어 주세요."
        : "힘이 약해졌어요 · 왼쪽 버튼을 눌러 감으세요.";

    private void Start()
    {
        WaterDeathZone waterZone = FindFirstObjectByType<WaterDeathZone>();
        if (waterZone != null) waterVolume = waterZone.GetComponent<Collider>();
        if (GetComponent<DeepBlueMenus>() == null) gameObject.AddComponent<DeepBlueMenus>();
        if (bobber != null)
        {
            bobber.gameObject.SetActive(false);
        }

        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.enabled = false;
        }

        if (viewCamera == null || rodTip == null || bobber == null || fishingLine == null)
        {
            Debug.LogError("FishingGameController: Scene Builder에서 카메라, 낚싯대 끝, 찌, 낚싯줄을 연결해야 합니다.", this);
        }

        if (GetComponent<FishingHUD>() == null)
        {
            gameObject.AddComponent<FishingHUD>();
        }

        PrepareGearVisuals();
        if (viewCamera != null)
        {
            GameObject fishView = new GameObject("Caught Fish 3D View");
            fishView.transform.SetParent(viewCamera, false);
            fishShowcase = fishView.AddComponent<DeepBlueFishShowcase>();
            fishShowcase.Hide();
        }
    }

    /// <summary>상점에서 장비를 바꾼 직후 1인칭 낚싯대와 찌 모양을 갱신합니다.</summary>
    public void RefreshGearVisuals()
    {
        if (rodRoot == null || rodTip == null) return;
        DeepBlueGearCatalog.RodDefinition rod = DeepBlueGearCatalog.FindRod(DeepBlueProfile.ActiveRodId);
        if (string.IsNullOrEmpty(rod.Id)) rod = DeepBlueGearCatalog.RodAt(0);

        Vector3 rootPosition = rodRoot.localPosition;
        rootPosition.y = -0.80f;
        rodRoot.localPosition = rootPosition;

        if (rodShaft != null)
        {
            Vector3 scale = rodShaftBaseScale;
            scale.y *= rod.LengthMultiplier;
            rodShaft.localScale = scale;
            Vector3 shaftPosition = rodShaftBasePosition;
            shaftPosition.y *= rod.LengthMultiplier;
            rodShaft.localPosition = shaftPosition;
            Renderer renderer = rodShaft.GetComponent<Renderer>();
            SetTint(renderer, rod.Color);
        }

        Vector3 tipPosition = rodTipBasePosition;
        tipPosition.y *= rod.LengthMultiplier;
        rodTip.localPosition = tipPosition;

        DeepBlueGearCatalog.BobberDefinition bobberStyle = DeepBlueGearCatalog.FindBobber(DeepBlueProfile.ActiveBobberId);
        if (string.IsNullOrEmpty(bobberStyle.Id)) bobberStyle = DeepBlueGearCatalog.BobberAt(0);
        if (bobberRenderers == null && bobber != null) bobberRenderers = bobber.GetComponentsInChildren<Renderer>(true);
        if (bobberRenderers != null)
        {
            foreach (Renderer renderer in bobberRenderers)
            {
                bool whitePart = renderer.name.Contains("White");
                Color tint = whitePart
                    ? bobberStyle.Id == "moonlight" ? new Color(0.64f, 0.9f, 0.94f) : new Color(0.94f, 0.94f, 0.88f)
                    : bobberStyle.Color;
                SetTint(renderer, tint);
            }
        }
    }

    private void PrepareGearVisuals()
    {
        gearPropertyBlock = new MaterialPropertyBlock();
        rodRoot = rodTip != null ? rodTip.parent : null;
        if (rodRoot != null)
        {
            rodShaft = rodRoot.Find("Rod Shaft");
            rodTipBasePosition = rodTip.localPosition;
            if (rodShaft != null)
            {
                rodShaftBaseScale = rodShaft.localScale;
                rodShaftBasePosition = rodShaft.localPosition;
            }
        }
        RefreshGearVisuals();
    }

    private void SetTint(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        if (gearPropertyBlock == null) gearPropertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(gearPropertyBlock);
        gearPropertyBlock.SetColor("_BaseColor", color);
        gearPropertyBlock.SetColor("_Color", color);
        renderer.SetPropertyBlock(gearPropertyBlock);
    }

    private void Update()
    {
        if (DeepBlueMenus.BlocksGameplay) return;
        Mouse mouse = Mouse.current;
        bool leftPressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool leftReleased = mouse != null && mouse.leftButton.wasReleasedThisFrame;
        bool leftHeld = mouse != null && mouse.leftButton.isPressed;

        switch (state)
        {
            case FishingState.Ready:
                if (leftPressed)
                {
                    castChargeSeconds = 0f;
                    state = FishingState.Charging;
                }
                break;

            case FishingState.Charging:
                if (leftHeld)
                {
                    castChargeSeconds = Mathf.Min(castChargeSeconds + Time.deltaTime, maxCastChargeSeconds);
                }

                if (leftReleased)
                {
                    StartCoroutine(CastBobber());
                }
                break;

            case FishingState.WaitingForBite:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    state = FishingState.Bite;
                    stateTimer = hookWindowSeconds;
                    Debug.Log($"입질! {hookWindowSeconds:0.#}초 안에 Space를 눌러 챔질하세요.", this);
                }
                break;

            case FishingState.Bite:
                stateTimer -= Time.deltaTime;
                bool hookPressed = leftPressed
                    || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
                if (hookPressed)
                {
                    ChooseFishReward();
                    fishWeightKg = DeepBlueFishDetails.RollWeight(ResultFishIcon);
                    fishPressure = DeepBlueFishDetails.FightPressure(ResultFishIcon, fishWeightKg);
                    fishEffort = FishEffort.Calm;
                    effortSeconds = Random.Range(1.6f, 2.4f);
                    state = FishingState.Reeling;
                    reelProgress = 0f;
                    lineTension = 0f;
                    Debug.Log("챔질 성공. 릴링을 시작하세요.", this);
                }
                else if (stateTimer <= 0f)
                {
                    FinishFishing(false, "챔질 시간이 지나 물고기를 놓쳤습니다.\n다음에는 입질 알림이 뜨면 Space 또는 왼쪽 버튼을 누르세요.");
                }
                break;

            case FishingState.Reeling:
                UpdateReeling(leftHeld);
                break;

            case FishingState.Caught:
            case FishingState.Failed:
                if (leftPressed && CanDismissResult)
                {
                    ResetFishing();
                }
                break;
        }

        UpdateFishingLine();
        UpdateBobberMotion();
    }

    private IEnumerator CastBobber()
    {
        if (viewCamera == null || rodTip == null || bobber == null || fishingLine == null)
        {
            state = FishingState.Failed;
            yield break;
        }

        Vector3 start = rodTip.position;
        if (!TryCalculateLandingPoint(out Vector3 finish))
        {
            FinishFishing(false, "가까운 수면을 찾지 못했습니다. 물가 쪽에서 다시 던져 주세요.");
            yield break;
        }

        state = FishingState.Casting;
        bobber.gameObject.SetActive(true);
        bobber.position = start;
        fishingLine.enabled = true;

        const float flightSeconds = 0.65f;
        float elapsed = 0f;
        while (elapsed < flightSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightSeconds);
            float arc = Mathf.Sin(t * Mathf.PI) * (1f + Mathf.Clamp01(castChargeSeconds / maxCastChargeSeconds));
            bobber.position = Vector3.Lerp(start, finish, t) + Vector3.up * arc;
            yield return null;
        }

        bobber.position = finish;
        bobberHomePosition = finish;
        hasBobberPosition = true;
        state = FishingState.WaitingForBite;
        DeepBlueGearCatalog.BobberDefinition bobberStyle = DeepBlueGearCatalog.FindBobber(DeepBlueProfile.ActiveBobberId);
        if (string.IsNullOrEmpty(bobberStyle.Id)) bobberStyle = DeepBlueGearCatalog.BobberAt(0);
        float biteScale = bobberStyle.BiteDelayMultiplier;
        stateTimer = Random.Range(minimumBiteDelaySeconds * biteScale,
            Mathf.Max(minimumBiteDelaySeconds, maximumBiteDelaySeconds) * biteScale);
    }

    private bool TryCalculateLandingPoint(out Vector3 point)
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(viewCamera.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = transform.forward;
        }

        float castPower = Mathf.Clamp01(castChargeSeconds / maxCastChargeSeconds);
        DeepBlueGearCatalog.RodDefinition rod = DeepBlueGearCatalog.FindRod(DeepBlueProfile.ActiveRodId);
        if (string.IsNullOrEmpty(rod.Id)) rod = DeepBlueGearCatalog.RodAt(0);
        float distance = Mathf.Lerp(6f, 20f, castPower) * rod.CastMultiplier;
        point = viewCamera.position + flatForward * distance;
        point.x = Mathf.Clamp(point.x, waterBoundsX.x, waterBoundsX.y);
        point.z = Mathf.Clamp(point.z, waterBoundsZ.x, waterBoundsZ.y);
        point.y = waterLevel + 0.12f;
        if (IsClearWater(point)) return true;

        // 사각형 수역 안에도 둔덕·바위가 있을 수 있어 실제 지면을 확인합니다.
        // 정상 착수는 그대로 유지하고, 막힌 위치만 최대 4m 안에서 보정합니다.
        Vector3 requestedPoint = point;
        for (float radius = 0.5f; radius <= 4f; radius += 0.5f)
        {
            for (int direction = 0; direction < 16; direction++)
            {
                float angle = direction * (Mathf.PI * 2f / 16f);
                Vector3 candidate = requestedPoint + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (candidate.x < waterBoundsX.x || candidate.x > waterBoundsX.y ||
                    candidate.z < waterBoundsZ.x || candidate.z > waterBoundsZ.y) continue;
                if (!IsClearWater(candidate)) continue;
                point = candidate;
                return true;
            }
        }

        return false;
    }

    private bool IsClearWater(Vector3 point)
    {
        if (waterVolume == null || !waterVolume.enabled || !waterVolume.gameObject.activeInHierarchy)
            return false;

        Vector3 surface = new Vector3(point.x, waterLevel, point.z);
        if ((waterVolume.ClosestPoint(surface) - surface).sqrMagnitude > 0.0001f) return false;

        // 수면용 Water 레이어와 Trigger는 제외하고 바닥·부두·바위를 검사합니다.
        int solidLayers = Physics.DefaultRaycastLayers & ~(1 << 4);
        return Physics.Raycast(surface + Vector3.up * 100f, Vector3.down, out RaycastHit hit,
                   200f, solidLayers, QueryTriggerInteraction.Ignore)
               && hit.point.y < waterLevel - 0.1f;
    }

    private void UpdateReeling(bool isHoldingReel)
    {
        // 게임 시간으로 진행되므로 메뉴를 열면 저항 예고와 주기도 함께 멈춥니다.
        effortSeconds -= Time.deltaTime;
        if (effortSeconds <= 0f)
        {
            if (fishEffort == FishEffort.Calm)
            {
                fishEffort = FishEffort.Warning;
                effortSeconds = 0.75f;
            }
            else if (fishEffort == FishEffort.Warning)
            {
                fishEffort = FishEffort.Surge;
                effortSeconds = Random.Range(1f, 1.5f);
            }
            else
            {
                fishEffort = FishEffort.Calm;
                effortSeconds = Random.Range(1.6f, 2.4f);
            }
        }
        if (isHoldingReel)
        {
            DeepBlueGearCatalog.RodDefinition rod = DeepBlueGearCatalog.FindRod(DeepBlueProfile.ActiveRodId);
            if (string.IsNullOrEmpty(rod.Id)) rod = DeepBlueGearCatalog.RodAt(0);
            reelProgress += reelProgressPerSecond * rod.ReelMultiplier * (IsFishSurging ? 0.65f : 1f) * Time.deltaTime;
            lineTension += tensionGainPerSecond * rod.TensionMultiplier * (IsFishSurging ? 1.45f : 0.65f) * fishPressure * Time.deltaTime;
        }
        else
        {
            lineTension -= tensionLossPerSecond * Time.deltaTime;
        }

        reelProgress = Mathf.Clamp01(reelProgress);
        lineTension = Mathf.Clamp01(lineTension);

        if (reelProgress >= 1f)
        {
            FinishFishing(true, "물고기를 낚았습니다!");
        }
        else if (lineTension >= 1f)
        {
            FinishFishing(false, "줄에 힘이 너무 많이 걸려 끊어졌습니다.\n다음에는 저항 예고나 장력 위험이 보이면 버튼을 놓으세요.");
        }
    }

    private void FinishFishing(bool caught, string message)
    {
        // 결과 프레임이 다시 호출되어도 저장이 두 번 증가하지 않게 막습니다.
        if (IsResultVisible) return;
        if (caught)
        {
            DeepBlueCollectionProgress before = DeepBlueCollectionProgress.Read(this);
            firstDiscovery = DeepBlueProfile.CatchCount(ResultFishIcon) == 0;
            float previousBest = DeepBlueProfile.BestWeight(ResultFishIcon);
            newWeightRecord = fishWeightKg > previousBest;
            DeepBlueProfile.RecordCatch(ResultFishIcon, fishWeightKg);
            if (fishShowcase != null) fishShowcase.Show(ResultFishIcon, fishWeightKg);
            DeepBlueCollectionProgress after = DeepBlueCollectionProgress.Read(this);
            completedGoal = after.Stage > before.Stage ? before.Title : string.Empty;
            string record = firstDiscovery ? "도감에 첫 기록을 남겼어요."
                : previousBest <= 0f ? "이 어종의 첫 무게 기록!"
                : newWeightRecord ? $"이전 최고 {DeepBlueFishDetails.FormatWeight(previousBest, ResultFishIcon)} kg를 넘었어요!"
                : $"개인 최고 {DeepBlueFishDetails.FormatWeight(previousBest, ResultFishIcon)} kg";
            message = $"{DeepBlueFishDetails.FormatWeight(fishWeightKg, ResultFishIcon)} kg  ·  {record}";
            if (!string.IsNullOrEmpty(completedGoal)) message += "\n목표 달성! " + completedGoal;
            else message += "\n" + after.Summary;
        }

        state = caught ? FishingState.Caught : FishingState.Failed;
        resultMessage = message;
        resultDismissAfter = Time.time + 0.8f;
        Debug.Log(message, this);
    }

    private void ChooseFishReward()
    {
        if (fishRewards != null && fishRewards.Length > 0)
        {
            float totalWeight = 0f;
            for (int i = 0; i < fishRewards.Length; i++)
                totalWeight += RewardSpawnWeight(fishRewards[i]);

            if (totalWeight <= 0f)
            {
                // 기존 장면의 직렬화된 물고기 목록은 가중치가 0이므로 계속 균등하게 나옵니다.
                currentFishReward = fishRewards[Random.Range(0, fishRewards.Length)];
            }
            else
            {
                float roll = Random.value * totalWeight;
                for (int i = 0; i < fishRewards.Length; i++)
                {
                    roll -= RewardSpawnWeight(fishRewards[i]);
                    if (roll <= 0f)
                    {
                        currentFishReward = fishRewards[i];
                        break;
                    }
                }
            }
        }
        else
        {
            currentFishReward.displayName = "무지개송어";
            currentFishReward.icon = catchFishIcon;
        }

        hasCurrentFishReward = true;
    }

    private static float RewardSpawnWeight(FishReward reward)
    {
        if (reward.spawnWeight > 0f && !float.IsNaN(reward.spawnWeight) && !float.IsInfinity(reward.spawnWeight))
            return reward.spawnWeight;
        return DeepBlueFishDetails.SpawnWeight(reward.icon);
    }

    private void ResetFishing()
    {
        StopAllCoroutines();
        castChargeSeconds = 0f;
        reelProgress = 0f;
        lineTension = 0f;
        hasBobberPosition = false;
        hasCurrentFishReward = false;
        fishWeightKg = 0f;
        firstDiscovery = false;
        newWeightRecord = false;
        completedGoal = string.Empty;
        effortSeconds = 0f;
        fishEffort = FishEffort.Calm;
        resultMessage = string.Empty;
        state = FishingState.Ready;

        if (bobber != null)
        {
            bobber.gameObject.SetActive(false);
        }

        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }

        if (fishShowcase != null) fishShowcase.Hide();
    }

    private string GetPrompt()
    {
        switch (state)
        {
            case FishingState.Ready:
                return "마우스 왼쪽 버튼을 누른 채 힘을 모으고 놓아 던지세요.";
            case FishingState.Charging:
                return "버튼을 놓으면 찌를 던집니다.";
            case FishingState.Casting:
                return "찌를 던지는 중입니다.";
            case FishingState.WaitingForBite:
                return "찌를 던졌습니다. 입질을 기다리세요.";
            case FishingState.Bite:
                return "입질! Space 또는 마우스 왼쪽 버튼으로 챔질하세요.";
            case FishingState.Reeling:
                return FightHint;
            case FishingState.Caught:
                return "물고기를 낚았습니다! 왼쪽 버튼으로 다시 시작하세요.";
            case FishingState.Failed:
                return "낚시에 실패했습니다. 왼쪽 버튼으로 다시 시작하세요.";
            default:
                return "낚시를 준비하고 있습니다.";
        }
    }

    private void UpdateFishingLine()
    {
        if (fishingLine == null || rodTip == null || bobber == null || !fishingLine.enabled || !bobber.gameObject.activeSelf)
        {
            return;
        }

        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, bobber.position);
    }

    private void UpdateBobberMotion()
    {
        if (!hasBobberPosition || bobber == null || !bobber.gameObject.activeSelf)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * (state == FishingState.Bite ? 18f : 2.5f)) * (state == FishingState.Bite ? 0.16f : 0.035f);
        bobber.position = bobberHomePosition + Vector3.up * bob;
    }

}

