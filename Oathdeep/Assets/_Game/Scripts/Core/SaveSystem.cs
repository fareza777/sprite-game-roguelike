using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Oathdeep.Core
{
    [Serializable]
    public sealed class SettingsData
    {
        public float music = 0.8f;
        public float sfx = 1f;
        public bool vibration = true;
        public float screenShake = 1f;
        public bool reduceMotion;
        public bool autoAim = true;
        public bool damageNumbers = true;
        public bool fixedJoystick;
        public bool leftHanded;
        // Player-placed touch controls: centres as fractions of the HUD safe area. Only used when customControls is set.
        public bool customControls;
        public Vector2 stickPos, attackPos, dodgePos, skillPos;
    }

    [Serializable]
    public sealed class QuestProgress
    {
        public string id;
        public int progress;
        public bool completed;
        public bool claimed;
    }

    /// <summary>Everything that survives between runs. Plain fields so JsonUtility can serialize it.</summary>
    /// <summary>Permanent bonuses bought with side-quest rewards. Applied to every hero at the start of a run.</summary>
    [Serializable]
    public sealed class MetaBonuses
    {
        public int health;
        public int attack;
        public int defense;
        public int startGold;
        public float crit;
        public float goldFind;
        public float skillHaste;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public SettingsData settings = new SettingsData();
        public bool introSeen;
        public bool onboardingDone;
        public bool adsRemoved;   // Remove Ads bought (or restored); re-confirmed from the store at startup
        public List<string> codex = new List<string>();
        public List<QuestProgress> quests = new List<QuestProgress>();
        public int bestFloor;
        public int runs;
        public int victories;
        public int deaths;
        public int totalKills;
        public string lastHero = "aldric";
        public List<string> items = new List<string>();   // item identities ever found (compendium)
        public List<string> heroesAtB3 = new List<string>();
        public MetaBonuses meta = new MetaBonuses();

        /// <summary>Returns true only the first time an item identity is found.</summary>
        public bool DiscoverItem(string id)
        {
            if (string.IsNullOrEmpty(id) || items.Contains(id)) return false;
            items.Add(id);
            return true;
        }

        /// <summary>Returns true only the first time an entry is unlocked (used to celebrate new codex pages).</summary>
        public bool UnlockCodex(string id)
        {
            if (string.IsNullOrEmpty(id) || codex.Contains(id)) return false;
            codex.Add(id);
            return true;
        }

        public bool IsCodexUnlocked(string id) => codex.Contains(id);

        public QuestProgress Quest(string id)
        {
            foreach (var q in quests) if (q.id == id) return q;
            var created = new QuestProgress { id = id };
            quests.Add(created);
            return created;
        }

        /// <summary>JsonUtility leaves missing objects null when reading older files.</summary>
        public SaveData Repaired()
        {
            settings ??= new SettingsData();
            codex ??= new List<string>();
            quests ??= new List<QuestProgress>();
            items ??= new List<string>();
            heroesAtB3 ??= new List<string>();
            meta ??= new MetaBonuses();
            if (string.IsNullOrEmpty(lastHero)) lastHero = "aldric";
            return this;
        }
    }

    /// <summary>Loads and atomically saves <see cref="SaveData"/> as JSON in persistentDataPath.</summary>
    public static class SaveSystem
    {
        const string FileName = "undersun_save.json";
        static SaveData current;

        public static SaveData Current => current ??= Load();

        static string PathOnDisk => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new SaveData();
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                return (data ?? new SaveData()).Repaired();
            }
            catch (ArgumentException)
            {
                return new SaveData();
            }
        }

        public static string ToJson(SaveData data) => JsonUtility.ToJson(data, true);

        public static SaveData Load()
        {
            try
            {
                return File.Exists(PathOnDisk) ? FromJson(File.ReadAllText(PathOnDisk)) : new SaveData();
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[Save] could not read save, starting fresh: {e.Message}");
                return new SaveData();
            }
        }

        /// <summary>Write to a temp file then swap, so a crash mid-write never corrupts the save.</summary>
        public static void Save()
        {
            if (current == null) return;
            string tmp = PathOnDisk + ".tmp";
            try
            {
                File.WriteAllText(tmp, ToJson(current));
                if (File.Exists(PathOnDisk)) File.Delete(PathOnDisk);
                File.Move(tmp, PathOnDisk);
            }
            catch (IOException e)
            {
                Debug.LogWarning($"[Save] write failed: {e.Message}");
            }
        }

        /// <summary>Wipes progress but keeps the player's settings.</summary>
        public static void ResetProgress()
        {
            var settings = Current.settings;
            current = new SaveData { settings = settings, introSeen = true };
            Save();
        }
    }
}
