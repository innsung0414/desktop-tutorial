using UnityEngine;

/// <summary>게임용 개체 크기 범위입니다. 실제 생태계 분포나 세계 기록을 뜻하지 않습니다.</summary>
public static class DeepBlueFishDetails
{
    // Stable profile ids for the two Fish Alive sample models.
    public const string GuppyIconId = "FishAlive_Guppy_CatchIcon";
    public const string ClownfishIconId = "FishAlive_Clownfish_CatchIcon";
    public const float UndiscoveredSpawnMultiplier = 4f;

    // Every 3D fish model shipped by the installed free assets appears in the active collection.
    // The two clownfish models are separate collectible looks because both are real model assets.
    public static readonly string[] CurrentSpeciesIds =
    {
        GuppyIconId,
        "Fish_Asset_Betta", "Fish_Asset_BlueGoldfish", "Fish_Asset_BlueTang", "Fish_Asset_ButterflyFish",
        "Fish_Asset_CardinalFish", "Fish_Asset_Goldfish", "Fish_Asset_MandarinFish", "Fish_Asset_MoorishIdol",
        "Fish_Asset_RoyalGramma", "Fish_Asset_Tang", "Fish_Asset_Tetra", "Fish_Asset_YellowTang",
        "Fish_Asset_ZebraClownFish", "Fish_Asset_Clownfish", ClownfishIconId,
        "Fish_Asset_ArmoredCatfish", "Fish_Asset_BlackLionFish", "Fish_Asset_Cowfish", "Fish_Asset_Flatfish",
        "Fish_Asset_FlowerHorn", "Fish_Asset_Lionfish", "Fish_Asset_Piranha", "Fish_Asset_Puffer",
        "Fish_Asset_Anglerfish", "Fish_Asset_Blobfish", "Fish_Asset_CoralGrouper", "Fish_Asset_Koi",
        "Fish_Asset_ParrotFish", "Fish_Asset_RedSnapper", "Fish_Asset_Turbot", "Fish_Asset_GoblinShark",
        "Fish_Asset_Humphead", "Fish_Asset_Shark", "Fish_Asset_Sunfish", "Fish_Asset_Swordfish", "Fish_Asset_Tuna"
    };

    // Fish roster stored by older builds. Keep these ids so old profile data and fish bags can be archived safely.
    public static readonly string[] LegacySpeciesIds =
    {
        "Trout_CatchIcon", "Fish_Bass", "Fish_Pike", "Fish_Bluegill", "Fish_Carp", "Fish_Catfish",
        "Fish_YellowPerch", "Fish_Walleye", "Fish_BrookTrout", "Fish_LakeSturgeon", "Fish_BlackCrappie", "Fish_Burbot",
        "Fish_Asset_Anglerfish", "Fish_Asset_ArmoredCatfish", "Fish_Asset_Betta", "Fish_Asset_BlackLionFish",
        "Fish_Asset_Blobfish", "Fish_Asset_BlueGoldfish", "Fish_Asset_BlueTang", "Fish_Asset_ButterflyFish",
        "Fish_Asset_CardinalFish", "Fish_Asset_Clownfish", "Fish_Asset_CoralGrouper", "Fish_Asset_Cowfish",
        "Fish_Asset_Flatfish", "Fish_Asset_FlowerHorn", "Fish_Asset_GoblinShark", "Fish_Asset_Goldfish",
        "Fish_Asset_Humphead", "Fish_Asset_Koi", "Fish_Asset_Lionfish", "Fish_Asset_MandarinFish",
        "Fish_Asset_MoorishIdol", "Fish_Asset_ParrotFish", "Fish_Asset_Piranha", "Fish_Asset_Puffer",
        "Fish_Asset_RedSnapper", "Fish_Asset_RoyalGramma", "Fish_Asset_Shark", "Fish_Asset_Sunfish",
        "Fish_Asset_Swordfish", "Fish_Asset_Tang", "Fish_Asset_Tetra", "Fish_Asset_Tuna",
        "Fish_Asset_Turbot", "Fish_Asset_YellowTang", "Fish_Asset_ZebraClownFish"
    };

    public static Vector2 WeightRange(Sprite icon)
    {
        return WeightRange(icon != null ? icon.name : string.Empty);
    }

    public static Vector2 WeightRange(string fishId)
    {
        switch (fishId)
        {
            case GuppyIconId: return new Vector2(0.003f, 0.03f);
            case ClownfishIconId: return new Vector2(0.04f, 0.25f);
            case "Trout_CatchIcon": return new Vector2(0.3f, 3.5f);
            case "Fish_Bass": return new Vector2(0.4f, 4.5f);
            case "Fish_Pike": return new Vector2(0.8f, 8f);
            case "Fish_Bluegill": return new Vector2(0.08f, 0.65f);
            case "Fish_Carp": return new Vector2(0.8f, 10f);
            case "Fish_Catfish": return new Vector2(0.6f, 8f);
            case "Fish_YellowPerch": return new Vector2(0.1f, 0.9f);
            case "Fish_Walleye": return new Vector2(0.5f, 5f);
            case "Fish_BrookTrout": return new Vector2(0.15f, 1.8f);
            case "Fish_LakeSturgeon": return new Vector2(3f, 25f);
            case "Fish_BlackCrappie": return new Vector2(0.12f, 1.2f);
            case "Fish_Burbot": return new Vector2(0.4f, 5f);
            case "Fish_Asset_Anglerfish": return new Vector2(4f, 60f);
            case "Fish_Asset_ArmoredCatfish": return new Vector2(0.5f, 6f);
            case "Fish_Asset_Betta": return new Vector2(0.003f, 0.02f);
            case "Fish_Asset_BlackLionFish": return new Vector2(0.5f, 2f);
            case "Fish_Asset_Blobfish": return new Vector2(2f, 12f);
            case "Fish_Asset_BlueGoldfish": return new Vector2(0.05f, 0.6f);
            case "Fish_Asset_BlueTang": return new Vector2(0.2f, 0.8f);
            case "Fish_Asset_ButterflyFish": return new Vector2(0.1f, 0.6f);
            case "Fish_Asset_CardinalFish": return new Vector2(0.01f, 0.05f);
            case "Fish_Asset_Clownfish": return new Vector2(0.05f, 0.25f);
            case "Fish_Asset_CoralGrouper": return new Vector2(1f, 20f);
            case "Fish_Asset_Cowfish": return new Vector2(0.5f, 7f);
            case "Fish_Asset_Flatfish": return new Vector2(0.5f, 10f);
            case "Fish_Asset_FlowerHorn": return new Vector2(0.5f, 3f);
            case "Fish_Asset_GoblinShark": return new Vector2(100f, 600f);
            case "Fish_Asset_Goldfish": return new Vector2(0.05f, 0.8f);
            case "Fish_Asset_Humphead": return new Vector2(15f, 180f);
            case "Fish_Asset_Koi": return new Vector2(0.5f, 25f);
            case "Fish_Asset_Lionfish": return new Vector2(0.5f, 2f);
            case "Fish_Asset_MandarinFish": return new Vector2(0.02f, 0.05f);
            case "Fish_Asset_MoorishIdol": return new Vector2(0.1f, 0.5f);
            case "Fish_Asset_ParrotFish": return new Vector2(1f, 30f);
            case "Fish_Asset_Piranha": return new Vector2(0.5f, 3f);
            case "Fish_Asset_Puffer": return new Vector2(0.2f, 10f);
            case "Fish_Asset_RedSnapper": return new Vector2(0.5f, 15f);
            case "Fish_Asset_RoyalGramma": return new Vector2(0.01f, 0.1f);
            case "Fish_Asset_Shark": return new Vector2(5f, 1000f);
            case "Fish_Asset_Sunfish": return new Vector2(300f, 1000f);
            case "Fish_Asset_Swordfish": return new Vector2(50f, 650f);
            case "Fish_Asset_Tang": return new Vector2(0.1f, 0.5f);
            case "Fish_Asset_Tetra": return new Vector2(0.002f, 0.02f);
            case "Fish_Asset_Tuna": return new Vector2(5f, 600f);
            case "Fish_Asset_Turbot": return new Vector2(2f, 25f);
            case "Fish_Asset_YellowTang": return new Vector2(0.1f, 0.5f);
            case "Fish_Asset_ZebraClownFish": return new Vector2(0.05f, 0.25f);
            default: return new Vector2(0.2f, 2f);
        }
    }

    public static string DisplayName(string fishId)
    {
        switch (fishId)
        {
            case GuppyIconId: return "구피";
            case ClownfishIconId: return "흰동가리 (Fish Alive)";
            case "Fish_Asset_Anglerfish": return "아귀";
            case "Fish_Asset_ArmoredCatfish": return "갑옷메기";
            case "Fish_Asset_Betta": return "베타";
            case "Fish_Asset_BlackLionFish": return "검은쏠배감펭";
            case "Fish_Asset_Blobfish": return "블롭피시";
            case "Fish_Asset_BlueGoldfish": return "푸른 금붕어";
            case "Fish_Asset_BlueTang": return "블루탱";
            case "Fish_Asset_ButterflyFish": return "나비고기";
            case "Fish_Asset_CardinalFish": return "카디널피시";
            case "Fish_Asset_Clownfish": return "흰동가리 (클래식)";
            case "Fish_Asset_CoralGrouper": return "산호그루퍼";
            case "Fish_Asset_Cowfish": return "뿔복";
            case "Fish_Asset_Flatfish": return "가자미";
            case "Fish_Asset_FlowerHorn": return "플라워혼";
            case "Fish_Asset_GoblinShark": return "고블린상어";
            case "Fish_Asset_Goldfish": return "금붕어";
            case "Fish_Asset_Humphead": return "혹돔";
            case "Fish_Asset_Koi": return "비단잉어";
            case "Fish_Asset_Lionfish": return "쏠배감펭";
            case "Fish_Asset_MandarinFish": return "만다린피시";
            case "Fish_Asset_MoorishIdol": return "무어리시 아이돌";
            case "Fish_Asset_ParrotFish": return "앵무고기";
            case "Fish_Asset_Piranha": return "피라냐";
            case "Fish_Asset_Puffer": return "복어";
            case "Fish_Asset_RedSnapper": return "붉은도미";
            case "Fish_Asset_RoyalGramma": return "로열그라마";
            case "Fish_Asset_Shark": return "상어";
            case "Fish_Asset_Sunfish": return "개복치";
            case "Fish_Asset_Swordfish": return "황새치";
            case "Fish_Asset_Tang": return "탱";
            case "Fish_Asset_Tetra": return "테트라";
            case "Fish_Asset_Tuna": return "참치";
            case "Fish_Asset_Turbot": return "넙치";
            case "Fish_Asset_YellowTang": return "옐로탱";
            case "Fish_Asset_ZebraClownFish": return "제브라 흰동가리";
            default: return fishId;
        }
    }

    public static string RarityLabel(string fishId)
    {
        float maximumWeight = WeightRange(fishId).y;
        if (maximumWeight <= 1f) return "일반";
        if (maximumWeight <= 10f) return "드묾";
        if (maximumWeight <= 60f) return "희귀";
        return "전설";
    }

    /// <summary>게임 밸런스용 출현 가중치입니다. 작은 어종은 더 자주, 대형 어종은 드물게 나옵니다.</summary>
    public static float SpawnWeight(Sprite icon) => SpawnWeight(icon != null ? icon.name : string.Empty);

    public static float SpawnWeight(string fishId)
    {
        float maximumWeight = WeightRange(fishId).y;
        if (maximumWeight <= 1f) return 4f;
        if (maximumWeight <= 10f) return 2.5f;
        if (maximumWeight <= 60f) return 1.35f;
        return 0.75f;
    }

    public static int WeightDecimals(Sprite icon) => WeightDecimals(icon != null ? icon.name : string.Empty);
    public static int WeightDecimals(string fishId) => WeightRange(fishId).y < 0.1f ? 3 : 2;

    public static string FormatWeight(float weightKg, Sprite icon)
    {
        return FormatWeight(weightKg, icon != null ? icon.name : string.Empty);
    }

    public static string FormatWeight(float weightKg, string fishId)
        => weightKg.ToString(WeightDecimals(fishId) == 3 ? "0.000" : "0.00");

    public static float RollWeight(Sprite icon)
    {
        Vector2 range = WeightRange(icon);
        // 보통 크기가 자주 나오고 큰 개체는 상대적으로 적습니다.
        float fraction = Mathf.Pow(Random.value, 1.8f);
        float precision = WeightDecimals(icon) == 3 ? 1000f : 100f;
        return Mathf.Round(Mathf.Lerp(range.x, range.y, fraction) * precision) / precision;
    }

    public static float FightPressure(Sprite icon, float weight)
    {
        Vector2 range = WeightRange(icon);
        return Mathf.Lerp(0.85f, 1.15f, Mathf.InverseLerp(range.x, range.y, weight));
    }
}
