using System;
using UnityEngine;

namespace Oathdeep.Core
{
    [Serializable]
    public sealed class StoryLine
    {
        public string id;
        public string text;
    }

    [Serializable]
    public sealed class FloorStory
    {
        public int floor;
        public string id;
        public string biome;
        public string verse;
        public string boss;
        public string entry;
        public string bossLine;
    }

    /// <summary>The narrative script (story.json): prologue, ending, and per-floor narration.</summary>
    [Serializable]
    public sealed class StoryData
    {
        public StoryLine[] prologue = new StoryLine[0];
        public StoryLine[] ending = new StoryLine[0];
        public FloorStory[] floors = new FloorStory[0];

        public static StoryData Parse(string json)
        {
            var data = string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<StoryData>(json);
            return data ?? new StoryData();
        }

        /// <summary>Floor 1..N; out-of-range numbers clamp to the nearest floor.</summary>
        public FloorStory Floor(int number)
        {
            if (floors.Length == 0) return null;
            return floors[Mathf.Clamp(number, 1, floors.Length) - 1];
        }
    }
}
