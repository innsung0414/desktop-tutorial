using UnityEngine;

/// <summary>이 PC에 설정과 어종별 획득 횟수만 저장합니다. 낚시 도중 상태는 저장하지 않습니다.</summary>
public static class DeepBlueProfile
{
    private const string Prefix = "DeepBlue.v1.";
    public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(Prefix + "Volume", 0.8f));
    public static float Sensitivity => Mathf.Clamp(PlayerPrefs.GetFloat(Prefix + "Sensitivity", 0.15f), 0.05f, 1f);

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

    public static void RecordCatch(Sprite fish)
    {
        if (fish == null) return;
        PlayerPrefs.SetInt(Prefix + "Fish." + fish.name, CatchCount(fish) + 1);
        Save();
    }

    public static void Save() => PlayerPrefs.Save();
}
