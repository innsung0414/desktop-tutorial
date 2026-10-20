using System.Collections.Generic;
using UnityEngine;

/// <summary>설정, 도감, 물고기 보관함, 화폐와 장비를 이 PC에 저장합니다.</summary>
public static class DeepBlueProfile
{
    private const string Prefix = "DeepBlue.v1.";
    private const int StartingCredits = 350;
    public static int CatchRevision { get; private set; }
    public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + "Volume", 0.8f));
    public static float Sensitivity => Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "Sensitivity", 0.15f), 0.05f, 1f);
    public static int Credits { get { EnsureEconomy(); return Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Credits", StartingCredits)); } }
    public static string ActiveRodId { get { EnsureEconomy(); return PlayerPrefs.GetString(Prefix + "Gear.Rod", "starter"); } }
    public static string ActiveBobberId { get { EnsureEconomy(); return PlayerPrefs.GetString(Prefix + "Gear.Bobber", "classic"); } }

    public static void SetVolume(float value)
    {
        PlayerPrefs.SetFloat(Prefix + "Volume", Mathf.Clamp01(value));
        AudioListener.volume = Volume;
    }

    public static void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat(Prefix + "Sensitivity", Mathf.Clamp(value, 0.05f, 1f));
    }

    // Sprite 이름을 키로 사용하므로 화면의 한글 이름을 고쳐도 기록이 유지됩니다.
    public static int CatchCount(Sprite fish) => fish == null ? 0 : Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Fish." + fish.name, 0));
    public static float BestWeight(Sprite fish) => fish == null ? 0f : Mathf.Max(0f, PlayerPrefs.GetFloat(Prefix + "BestKg." + fish.name, 0f));
    public static int InventoryCount(Sprite fish) => fish == null ? 0 : Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Bag.Count." + fish.name, 0));
    public static float InventoryWeightKg(Sprite fish) => fish == null ? 0f : Mathf.Max(0f, PlayerPrefs.GetFloat(Prefix + "Bag.Kg." + fish.name, 0f));

    /// <summary>이전 어종 ID를 현재 도감과 겹치지 않게 합산합니다.</summary>
    public static int ArchivedCatchCount(Sprite[] activeSpecies)
    {
        int total = 0;
        foreach (string fishId in ArchivedSpeciesIds(activeSpecies))
            total += Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Fish." + fishId, 0));
        return total;
    }

    public static int ArchivedDiscoveredSpeciesCount(Sprite[] activeSpecies)
    {
        int discovered = 0;
        foreach (string fishId in ArchivedSpeciesIds(activeSpecies))
            if (PlayerPrefs.GetInt(Prefix + "Fish." + fishId, 0) > 0) discovered++;
        return discovered;
    }

    public static int ArchivedInventoryCount(Sprite[] activeSpecies)
    {
        int total = 0;
        foreach (string fishId in ArchivedSpeciesIds(activeSpecies))
            total += Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Bag.Count." + fishId, 0));
        return total;
    }

    public static bool OwnsRod(string id)
    {
        EnsureEconomy();
        return !string.IsNullOrEmpty(id) && PlayerPrefs.GetInt(Prefix + "Owned.Rod." + id, 0) == 1;
    }

    public static bool OwnsBobber(string id)
    {
        EnsureEconomy();
        return !string.IsNullOrEmpty(id) && PlayerPrefs.GetInt(Prefix + "Owned.Bobber." + id, 0) == 1;
    }

    public static bool TryBuyRod(string id)
    {
        EnsureEconomy();
        DeepBlueGearCatalog.RodDefinition rod = DeepBlueGearCatalog.FindRod(id);
        if (rod.Id != id || OwnsRod(id) || Credits < rod.Price) return false;
        Spend(rod.Price);
        PlayerPrefs.SetInt(Prefix + "Owned.Rod." + id, 1);
        PlayerPrefs.SetString(Prefix + "Gear.Rod", id);
        Save();
        return true;
    }

    public static bool TryBuyBobber(string id)
    {
        EnsureEconomy();
        DeepBlueGearCatalog.BobberDefinition bobber = DeepBlueGearCatalog.FindBobber(id);
        if (bobber.Id != id || OwnsBobber(id) || Credits < bobber.Price) return false;
        Spend(bobber.Price);
        PlayerPrefs.SetInt(Prefix + "Owned.Bobber." + id, 1);
        PlayerPrefs.SetString(Prefix + "Gear.Bobber", id);
        Save();
        return true;
    }

    public static bool EquipRod(string id)
    {
        if (!OwnsRod(id)) return false;
        PlayerPrefs.SetString(Prefix + "Gear.Rod", id);
        Save();
        return true;
    }

    public static bool EquipBobber(string id)
    {
        if (!OwnsBobber(id)) return false;
        PlayerPrefs.SetString(Prefix + "Gear.Bobber", id);
        Save();
        return true;
    }

    public static void RecordCatch(Sprite fish, float weightKg = 0f)
    {
        if (fish == null) return;
        EnsureEconomy();
        weightKg = !float.IsNaN(weightKg) && !float.IsInfinity(weightKg) ? Mathf.Max(0f, weightKg) : 0f;
        PlayerPrefs.SetInt(Prefix + "Fish." + fish.name, CatchCount(fish) + 1);
        if (weightKg > BestWeight(fish))
            PlayerPrefs.SetFloat(Prefix + "BestKg." + fish.name, weightKg);
        PlayerPrefs.SetInt(Prefix + "Bag.Count." + fish.name, InventoryCount(fish) + 1);
        PlayerPrefs.SetFloat(Prefix + "Bag.Kg." + fish.name, InventoryWeightKg(fish) + weightKg);
        CatchRevision++;
        Save();
    }

    /// <summary>현재 보관함의 물고기를 모두 팔고, 도감 기록은 그대로 둡니다.</summary>
    public static int SellAllFish(Sprite[] species, out int fishCount)
    {
        EnsureEconomy();
        fishCount = 0;
        float totalValue = 0f;
        var activeIds = new HashSet<string>();
        if (species != null)
        {
            foreach (Sprite fish in species)
            {
                if (fish == null) continue;
                activeIds.Add(fish.name);
                int count = InventoryCount(fish);
                if (count <= 0) continue;
                float weight = InventoryWeightKg(fish);
                totalValue += Mathf.Max(count * 4f, weight * DeepBlueGearCatalog.SellCreditsPerKg(fish));
                fishCount += count;
                PlayerPrefs.SetInt(Prefix + "Bag.Count." + fish.name, 0);
                PlayerPrefs.SetFloat(Prefix + "Bag.Kg." + fish.name, 0f);
            }
        }

        // 새 어종으로 교체된 뒤에도 이전 빌드에서 보관 중이던 물고기를 판매할 수 있습니다.
        foreach (string fishId in DeepBlueFishDetails.LegacySpeciesIds)
        {
            if (activeIds.Contains(fishId)) continue;
            int count = Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "Bag.Count." + fishId, 0));
            if (count <= 0) continue;
            float weight = Mathf.Max(0f, PlayerPrefs.GetFloat(Prefix + "Bag.Kg." + fishId, 0f));
            totalValue += Mathf.Max(count * 4f, weight * DeepBlueGearCatalog.SellCreditsPerKg(fishId));
            fishCount += count;
            PlayerPrefs.SetInt(Prefix + "Bag.Count." + fishId, 0);
            PlayerPrefs.SetFloat(Prefix + "Bag.Kg." + fishId, 0f);
        }

        int payout = fishCount > 0 ? Mathf.Max(1, Mathf.RoundToInt(totalValue)) : 0;
        if (payout > 0) PlayerPrefs.SetInt(Prefix + "Credits", Credits + payout);
        Save();
        return payout;
    }

    private static IEnumerable<string> ArchivedSpeciesIds(Sprite[] activeSpecies)
    {
        var activeIds = new HashSet<string>();
        if (activeSpecies != null)
            foreach (Sprite fish in activeSpecies)
                if (fish != null) activeIds.Add(fish.name);

        foreach (string fishId in DeepBlueFishDetails.LegacySpeciesIds)
            if (!activeIds.Contains(fishId)) yield return fishId;
    }

    private static void Spend(int amount)
    {
        PlayerPrefs.SetInt(Prefix + "Credits", Mathf.Max(0, Credits - amount));
    }

    private static void EnsureEconomy()
    {
        string initializedKey = Prefix + "EconomyInitialized";
        if (PlayerPrefs.GetInt(initializedKey, 0) == 1) return;
        PlayerPrefs.SetInt(Prefix + "Credits", StartingCredits);
        PlayerPrefs.SetInt(Prefix + "Owned.Rod.starter", 1);
        PlayerPrefs.SetInt(Prefix + "Owned.Bobber.classic", 1);
        PlayerPrefs.SetString(Prefix + "Gear.Rod", "starter");
        PlayerPrefs.SetString(Prefix + "Gear.Bobber", "classic");
        PlayerPrefs.SetInt(initializedKey, 1);
        Save();
    }

    public static void Save() => PlayerPrefs.Save();
}
