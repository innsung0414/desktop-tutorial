using UnityEngine;

/// <summary>저장된 도감 횟수에서 목표를 계산하므로 별도 퀘스트 저장이 필요 없습니다.</summary>
public readonly struct DeepBlueCollectionProgress
{
    public readonly int Stage;
    public readonly string Title;
    public readonly int Current;
    public readonly int Target;
    public bool IsComplete => Stage == 6;
    public float Normalized => Target > 0 ? Mathf.Clamp01(Current / (float)Target) : 1f;
    public string Summary => IsComplete ? "도감 목표를 모두 달성했습니다! 최고 무게에 도전해 보세요." : $"{Title}  ·  {Current}/{Target}";

    private DeepBlueCollectionProgress(int stage, string title, int current, int target)
    {
        Stage = stage; Title = title; Current = current; Target = target;
    }

    public static DeepBlueCollectionProgress Read(FishingGameController fishing)
    {
        int total = 0, discovered = 0;
        for (int i = 0; fishing != null && i < fishing.FishSpeciesCount; i++)
        {
            int count = DeepBlueProfile.CatchCount(fishing.FishSpeciesIcon(i));
            total += count;
            if (count > 0) discovered++;
        }
        return FromCounts(total, discovered, fishing != null ? fishing.FishSpeciesCount : DeepBlueFishDetails.CurrentSpeciesIds.Length);
    }

    public static DeepBlueCollectionProgress FromCounts(int total, int discovered)
    {
        return FromCounts(total, discovered, DeepBlueFishDetails.CurrentSpeciesIds.Length);
    }

    public static DeepBlueCollectionProgress FromCounts(int total, int discovered, int speciesCount)
    {
        speciesCount = Mathf.Max(1, speciesCount);
        if (total < 1) return new DeepBlueCollectionProgress(0, "첫 물고기 낚기", total, 1);
        int firstTarget = Mathf.Min(3, speciesCount);
        if (discovered < firstTarget)
            return new DeepBlueCollectionProgress(1, $"서로 다른 물고기 {firstTarget}종 발견", discovered, firstTarget);
        if (total < 5) return new DeepBlueCollectionProgress(2, "물고기 총 5마리 낚기", total, 5);
        int halfTarget = Mathf.CeilToInt(speciesCount * 0.5f);
        if (discovered < halfTarget)
            return new DeepBlueCollectionProgress(3, $"도감 절반 채우기 · {halfTarget}종", discovered, halfTarget);
        int nearCompleteTarget = Mathf.CeilToInt(speciesCount * 0.8f);
        if (discovered < nearCompleteTarget)
            return new DeepBlueCollectionProgress(4, $"도감 80% 채우기 · {nearCompleteTarget}종", discovered, nearCompleteTarget);
        if (discovered < speciesCount)
            return new DeepBlueCollectionProgress(5, $"전체 {speciesCount}개 항목 발견", discovered, speciesCount);
        return new DeepBlueCollectionProgress(6, "Deep Blue 도감 완성", speciesCount, speciesCount);
    }
}
