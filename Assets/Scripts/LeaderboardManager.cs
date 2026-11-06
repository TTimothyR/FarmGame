using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LeaderboardManager
{
    public static string CurrentPlayerName;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

    private static LBEntryData LoadLeaderboard()
    {
        if (!File.Exists(FilePath))
        {
            return new LBEntryData();
        }

        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<LBEntryData>(json) ?? new LBEntryData();
    }

    public static void SaveScore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        LBEntryData data = LoadLeaderboard();

        Entry existingEntry =
            data.entries.Find(entry => entry.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        if (existingEntry != null)
        {
            if (score > existingEntry.score)
            {
                existingEntry.score = score;
            }
        }
        else
        {
            data.entries.Add(new Entry(playerName, score));
        }
        
        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
    }

    public static List<Entry> GetTopScores(int count = 10)
    {
        LBEntryData data = LoadLeaderboard();

        int limit = Mathf.Min(count, data.entries.Count);
        return data.entries.GetRange(0, limit);
    }

    public static int GetPlayerHighScore(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return 0;
        }

        LBEntryData data = LoadLeaderboard();

        Entry playerEntry =
            data.entries.Find(entry => entry.playerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        return playerEntry?.score ?? 0;
    }
}