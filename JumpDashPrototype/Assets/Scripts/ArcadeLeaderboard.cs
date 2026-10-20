using System;
using System.Collections.Generic;
using UnityEngine;

public static class ArcadeLeaderboard
{
    public const int MaxEntries = 5;
    public const int NicknameLimit = 12;

    private const string LeaderboardKey = "ArcadeLeaderboardTopFiveV1";
    private const string LegacyBestScoreKey = "BestScore";

    [Serializable]
    public sealed class ScoreEntry
    {
        public string nickname;
        public int scoreSeconds;

        public ScoreEntry()
        {
        }

        public ScoreEntry(string nickname, int scoreSeconds)
        {
            this.nickname = nickname;
            this.scoreSeconds = scoreSeconds;
        }
    }

    [Serializable]
    private sealed class SaveData
    {
        public ScoreEntry[] entries;
    }

    public static List<ScoreEntry> Load()
    {
        if (!PlayerPrefs.HasKey(LeaderboardKey))
        {
            List<ScoreEntry> migrated = new List<ScoreEntry>();
            int legacyBest = Mathf.FloorToInt(PlayerPrefs.GetFloat(LegacyBestScoreKey, 0f));
            if (legacyBest > 0)
            {
                migrated.Add(new ScoreEntry("PLAYER", legacyBest));
                Save(migrated);
            }

            return migrated;
        }

        string json = PlayerPrefs.GetString(LeaderboardKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return new List<ScoreEntry>();

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        List<ScoreEntry> scores = new List<ScoreEntry>();
        if (data == null || data.entries == null)
            return scores;

        for (int i = 0; i < data.entries.Length; i++)
        {
            ScoreEntry entry = data.entries[i];
            if (entry == null || entry.scoreSeconds < 0)
                continue;

            string nickname = CleanNickname(entry.nickname);
            if (nickname.Length == 0)
                continue;

            InsertSorted(scores, new ScoreEntry(nickname, entry.scoreSeconds));
            if (scores.Count > MaxEntries)
                scores.RemoveAt(scores.Count - 1);
        }

        return scores;
    }

    public static int GetQualifyingRank(int scoreSeconds, IList<ScoreEntry> currentScores)
    {
        if (currentScores == null)
            return 1;

        int insertionIndex = 0;
        while (insertionIndex < currentScores.Count &&
               currentScores[insertionIndex].scoreSeconds >= scoreSeconds)
        {
            insertionIndex++;
        }

        return insertionIndex < MaxEntries ? insertionIndex + 1 : -1;
    }

    public static bool Submit(string nickname, int scoreSeconds, out int rank)
    {
        List<ScoreEntry> scores = Load();
        rank = GetQualifyingRank(scoreSeconds, scores);
        string cleanNickname = CleanNickname(nickname);
        if (rank < 1 || cleanNickname.Length == 0)
            return false;

        scores.Insert(rank - 1, new ScoreEntry(cleanNickname, Mathf.Max(0, scoreSeconds)));
        if (scores.Count > MaxEntries)
            scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);

        Save(scores);
        return true;
    }

    public static int BestScoreSeconds(IList<ScoreEntry> scores)
    {
        return scores != null && scores.Count > 0 ? scores[0].scoreSeconds : 0;
    }

    private static void InsertSorted(List<ScoreEntry> scores, ScoreEntry entry)
    {
        int index = 0;
        while (index < scores.Count && scores[index].scoreSeconds >= entry.scoreSeconds)
            index++;

        scores.Insert(index, entry);
    }

    private static string CleanNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            return string.Empty;

        string clean = nickname.Trim();
        return clean.Length > NicknameLimit ? clean.Substring(0, NicknameLimit) : clean;
    }

    private static void Save(List<ScoreEntry> scores)
    {
        SaveData data = new SaveData { entries = scores.ToArray() };
        PlayerPrefs.SetString(LeaderboardKey, JsonUtility.ToJson(data));
        PlayerPrefs.SetFloat(LegacyBestScoreKey, BestScoreSeconds(scores));
        PlayerPrefs.Save();
    }
}
