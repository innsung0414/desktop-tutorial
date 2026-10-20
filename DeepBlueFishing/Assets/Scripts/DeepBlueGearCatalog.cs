using System;
using UnityEngine;

/// <summary>첫 상점에서 판매하는 장비와 게임 효과를 한곳에 모아 둡니다.</summary>
public static class DeepBlueGearCatalog
{
    public readonly struct RodDefinition
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly int Price;
        public readonly float LengthMultiplier;
        public readonly float CastMultiplier;
        public readonly float ReelMultiplier;
        public readonly float TensionMultiplier;
        public readonly Color Color;

        public RodDefinition(string id, string name, string description, int price,
            float lengthMultiplier, float castMultiplier, float reelMultiplier,
            float tensionMultiplier, Color color)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            LengthMultiplier = lengthMultiplier;
            CastMultiplier = castMultiplier;
            ReelMultiplier = reelMultiplier;
            TensionMultiplier = tensionMultiplier;
            Color = color;
        }
    }

    public readonly struct BobberDefinition
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly int Price;
        public readonly float BiteDelayMultiplier;
        public readonly Color Color;

        public BobberDefinition(string id, string name, string description, int price,
            float biteDelayMultiplier, Color color)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            BiteDelayMultiplier = biteDelayMultiplier;
            Color = color;
        }
    }

    private static readonly RodDefinition[] Rods =
    {
        new RodDefinition("starter", "호숫가 기본 낚싯대", "기본 장비 · 편안한 시작", 0, 1f, 1f, 1f, 1f,
            new Color(0.13f, 0.17f, 0.20f)),
        new RodDefinition("streamglass", "스트림글라스", "캐스팅 거리 +8% · 릴링 +8%", 190, 1.04f, 1.08f, 1.08f, 0.95f,
            new Color(0.50f, 0.34f, 0.19f)),
        new RodDefinition("carbon", "블루 카본 프로", "캐스팅 거리 +18% · 릴링 +15% · 장력 부담 -15%", 520, 1.10f, 1.18f, 1.15f, 0.85f,
            new Color(0.10f, 0.48f, 0.62f))
    };

    private static readonly BobberDefinition[] Bobbers =
    {
        new BobberDefinition("classic", "클래식 빨강·흰색 찌", "기본 찌 · 익숙한 시인성", 0, 1f,
            new Color(0.9f, 0.2f, 0.13f)),
        new BobberDefinition("sunrise", "선라이즈 찌", "따뜻한 주황색 몸체 · 입질 대기 -5%", 75, 0.95f,
            new Color(1f, 0.48f, 0.08f)),
        new BobberDefinition("moonlight", "문라이트 찌", "밝은 청록색 몸체 · 입질 대기 -12%", 145, 0.88f,
            new Color(0.20f, 0.92f, 0.86f))
    };

    public static int RodCount => Rods.Length;
    public static int BobberCount => Bobbers.Length;
    public static RodDefinition RodAt(int index) => Rods[Mathf.Clamp(index, 0, Rods.Length - 1)];
    public static BobberDefinition BobberAt(int index) => Bobbers[Mathf.Clamp(index, 0, Bobbers.Length - 1)];

    public static RodDefinition FindRod(string id)
    {
        foreach (RodDefinition rod in Rods)
            if (rod.Id == id) return rod;
        return default;
    }

    public static BobberDefinition FindBobber(string id)
    {
        foreach (BobberDefinition bobber in Bobbers)
            if (bobber.Id == id) return bobber;
        return default;
    }

    public static float SellCreditsPerKg(Sprite fish)
    {
        string fishName = fish != null ? fish.name : string.Empty;
        return SellCreditsPerKg(fishName);
    }

    public static float SellCreditsPerKg(string fishName)
    {
        fishName = fishName ?? string.Empty;
        if (fishName.StartsWith("Fish_Asset_", StringComparison.Ordinal))
        {
            // Free-pack species use different game weight scales. Normalize a typical catch to about 35 C,
            // while the four-credit per-fish floor keeps tiny species worth selling.
            float maximumWeight = DeepBlueFishDetails.WeightRange(fishName).y;
            return maximumWeight > 0f ? Mathf.Clamp(125f / maximumWeight, 0.1f, 24f) : 18f;
        }

        switch (fishName)
        {
            case DeepBlueFishDetails.GuppyIconId: return 1600f;
            case DeepBlueFishDetails.ClownfishIconId: return 240f;
            case "Trout_CatchIcon": return 22f;
            case "Fish_Bass": return 21f;
            case "Fish_Pike": return 20f;
            case "Fish_Bluegill": return 25f;
            case "Fish_Carp": return 16f;
            case "Fish_Catfish": return 18f;
            case "Fish_YellowPerch": return 24f;
            case "Fish_Walleye": return 26f;
            case "Fish_BrookTrout": return 24f;
            case "Fish_LakeSturgeon": return 22f;
            case "Fish_BlackCrappie": return 25f;
            case "Fish_Burbot": return 21f;
            default: return 18f;
        }
    }
}
