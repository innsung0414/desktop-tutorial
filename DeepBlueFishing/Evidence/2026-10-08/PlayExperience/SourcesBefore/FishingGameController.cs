using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public string ResultTitle => DidCatchFish ? "낚시 성공" : "낚시 실패";
    public string ResultMessage => string.IsNullOrEmpty(resultMessage) ? "왼쪽 버튼을 눌러 다시 낚시하세요." : resultMessage;
    public string ResultFishName => hasCurrentFishReward && !string.IsNullOrWhiteSpace(currentFishReward.displayName)
        ? currentFishReward.displayName
        : "무지개송어";
    public Sprite ResultFishIcon => hasCurrentFishReward ? currentFishReward.icon : catchFishIcon;
    public int FishSpeciesCount => fishRewards != null ? fishRewards.Length : 0;
    public string FishSpeciesName(int index) => fishRewards[index].displayName;
    public Sprite FishSpeciesIcon(int index) => fishRewards[index].icon;

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
                    state = FishingState.Reeling;
                    reelProgress = 0f;
                    lineTension = 0f;
                    Debug.Log("챔질 성공. 릴링을 시작하세요.", this);
                }
                else if (stateTimer <= 0f)
                {
                    FinishFishing(false, "챔질 시간이 지나 물고기를 놓쳤습니다.");
                }
                break;

            case FishingState.Reeling:
                UpdateReeling(leftHeld);
                break;

            case FishingState.Caught:
            case FishingState.Failed:
                if (leftPressed)
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
        stateTimer = Random.Range(minimumBiteDelaySeconds, Mathf.Max(minimumBiteDelaySeconds, maximumBiteDelaySeconds));
    }

    private bool TryCalculateLandingPoint(out Vector3 point)
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(viewCamera.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = transform.forward;
        }

        float castPower = Mathf.Clamp01(castChargeSeconds / maxCastChargeSeconds);
        float distance = Mathf.Lerp(6f, 20f, castPower);
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
        if (isHoldingReel)
        {
            reelProgress += reelProgressPerSecond * Time.deltaTime;
            lineTension += tensionGainPerSecond * Time.deltaTime;
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
            FinishFishing(false, "줄에 힘이 너무 많이 걸려 끊어졌습니다.");
        }
    }

    private void FinishFishing(bool caught, string message)
    {
        if (caught)
        {
            ChooseFishReward();
            DeepBlueProfile.RecordCatch(ResultFishIcon);
        }

        state = caught ? FishingState.Caught : FishingState.Failed;
        resultMessage = message;
        Debug.Log(message, this);
    }

    private void ChooseFishReward()
    {
        if (fishRewards != null && fishRewards.Length > 0)
        {
            currentFishReward = fishRewards[Random.Range(0, fishRewards.Length)];
        }
        else
        {
            currentFishReward.displayName = "무지개송어";
            currentFishReward.icon = catchFishIcon;
        }

        hasCurrentFishReward = true;
    }

    private void ResetFishing()
    {
        StopAllCoroutines();
        castChargeSeconds = 0f;
        reelProgress = 0f;
        lineTension = 0f;
        hasBobberPosition = false;
        hasCurrentFishReward = false;
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
                return "왼쪽 버튼을 눌러 감고, 줄 장력이 높으면 놓으세요.";
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

