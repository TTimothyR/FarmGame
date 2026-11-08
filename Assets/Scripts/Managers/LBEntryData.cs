using System;
using System.Collections.Generic;
/// <summary>
/// The data that is stored in the JSON leaderboard file.
/// </summary>
[Serializable]
public class Entry
{
    public string playerName;
    public int score;

    public Entry(string name, int score)
    {
        playerName = name;
        this.score = score;
    }
}

[Serializable]
public class LBEntryData
{
    public List<Entry> entries = new List<Entry>();
}
