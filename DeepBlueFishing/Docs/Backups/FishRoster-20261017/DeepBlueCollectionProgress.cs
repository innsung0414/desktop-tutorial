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
        return FromCounts(total, discovered, fishing != null ? fishing.FishSpeciesCount : 47);
    }

    public static DeepBlueCollectionProgress FromCounts(int total, int discovered)
    {
        return FromCounts(total, discovered, 47);
    }

    public static DeepBlueCollectionProgress FromCounts(int total, int discovered, int speciesCount)
    {
        if (speciesCount <= 2)
        {
            int speciesGoal = Mathf.Max(1, speciesCount);
            if (total < 1) return new DeepBlueCollectionProgress(0, "첫 물고기 낚기", total, 1);
            if (discovered < speciesGoal)
                return new DeepBlueCollectionProgress(1, "두 어종 모두 발견", discovered, speciesGoal);
            if (total < 5) return new DeepBlueCollectionProgress(2, "물고기 총 5마리 낚기", total, 5);
            if (total < 10) return new DeepBlueCollectionProgress(3, "물고기 총 10마리 낚기", total, 10);
            if (total < 20) return new DeepBlueCollectionProgress(4, "물고기 총 20마리 낚기", total, 20);
            if (total < 30) return new DeepBlueCollectionProgress(5, "물고기 총 30마리 낚기", total, 30);
            return new DeepBlueCollectionProgress(6, "두 어종 도감 완성", 30, 30);
        }

        if (total < 1) return new DeepBlueCollectionProgress(0, "첫 물고기 낚기", total, 1);
        if (discovered < 3) return new DeepBlueCollectionProgress(1, "서로 다른 물고기 3종 발견", discovered, 3);
        if (total < 5) return new DeepBlueCollectionProgress(2, "물고기 총 5마리 낚기", total, 5);
        if (discovered < 6) return new DeepBlueCollectionProgress(3, "도감 절반 채우기 · 6종", discovered, 6);
        if (discovered < 24) return new DeepBlueCollectionProgress(4, "전체 47종 중 24종 발견", discovered, 24);
        if (discovered < 47) return new DeepBlueCollectionProgress(5, "전체 47종 모두 발견", discovered, 47);
        return new DeepBlueCollectionProgress(6, "Deep Blue 도감 완성", 47, 47);
    }
}
