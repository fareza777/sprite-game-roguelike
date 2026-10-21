using System.Collections.Generic;
using System.Linq;
using Oathdeep.Data;

namespace Oathdeep.Core
{
    /// <summary>What advances a side quest.</summary>
    public enum QuestTrigger { Kills, EliteKills, ChestsOpened, Purchases, PilgrimsMet, SunstonesTouched, DiceRolled, ItemsFound, LegendariesFound, HeroesAtB3, ChoristersSilenced }

    /// <summary>A permanent bonus earned by claiming a side quest; it applies to every hero on every later run.</summary>
    public readonly struct MetaReward
    {
        public readonly int Health, Attack, Defense, StartGold;
        public readonly float Crit, GoldFind, SkillHaste;

        public MetaReward(int health = 0, int attack = 0, int defense = 0, int startGold = 0, float crit = 0f, float goldFind = 0f, float skillHaste = 0f)
        {
            Health = health; Attack = attack; Defense = defense; StartGold = startGold; Crit = crit; GoldFind = goldFind; SkillHaste = skillHaste;
        }

        public string Describe()
        {
            var parts = new List<string>();
            if (Health > 0) parts.Add($"+{Health} Health");
            if (Attack > 0) parts.Add($"+{Attack} Attack");
            if (Defense > 0) parts.Add($"+{Defense} Defense");
            if (Crit > 0) parts.Add($"+{Crit * 100f:0}% Crit");
            if (GoldFind > 0) parts.Add($"+{GoldFind * 100f:0}% Gold find");
            if (SkillHaste > 0) parts.Add($"+{SkillHaste * 100f:0}% Skill haste");
            if (StartGold > 0) parts.Add($"+{StartGold} starting gold");
            return string.Join(", ", parts) + " for every descent";
        }
    }

    public sealed class SideQuest
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public QuestTrigger Trigger { get; }
        public int Goal { get; }
        public MetaReward Reward { get; }

        public SideQuest(string id, string title, string description, QuestTrigger trigger, int goal, MetaReward reward)
        {
            Id = id; Title = title; Description = description; Trigger = trigger; Goal = goal; Reward = reward;
        }
    }

    /// <summary>One chapter of the main story: the prologue, one per Chorister, and the epilogue.</summary>
    public sealed class Chapter
    {
        public string Id { get; }
        public string Title { get; }
        public string Goal { get; }
        public string Journal { get; }   // revealed once the chapter is done

        public Chapter(string id, string title, string goal, string journal)
        {
            Id = id; Title = title; Goal = goal; Journal = journal;
        }
    }

    public static class QuestCatalog
    {
        static MetaReward R(int hp = 0, int atk = 0, int def = 0, int gold = 0, float crit = 0f, float find = 0f, float haste = 0f) =>
            new MetaReward(hp, atk, def, gold, crit, find, haste);

        public static readonly IReadOnlyList<SideQuest> Side = new[]
        {
            new SideQuest("first_blood", "First Blood", "Defeat 50 creatures of the deep.", QuestTrigger.Kills, 50, R(hp: 5)),
            new SideQuest("exterminator", "Exterminator", "Defeat 500 creatures of the deep.", QuestTrigger.Kills, 500, R(atk: 2)),
            new SideQuest("elite_hunter", "Elite Hunter", "Defeat 25 elite creatures.", QuestTrigger.EliteKills, 25, R(atk: 2)),
            new SideQuest("treasure_hunter", "Treasure Hunter", "Open 20 chests.", QuestTrigger.ChestsOpened, 20, R(gold: 25)),
            new SideQuest("tallow_patron", "Patron of Tallow", "Buy 10 wares from Old Tallow.", QuestTrigger.Purchases, 10, R(find: 0.05f)),
            new SideQuest("pilgrims_friend", "Pilgrim's Friend", "Speak with 8 pilgrims of the Sun.", QuestTrigger.PilgrimsMet, 8, R(hp: 5)),
            new SideQuest("stone_seeker", "Stone Seeker", "Draw light from 10 sunstones.", QuestTrigger.SunstonesTouched, 10, R(haste: 0.05f)),
            new SideQuest("gambler", "Bone Dice", "Roll the gambler's dice 5 times.", QuestTrigger.DiceRolled, 5, R(gold: 25)),
            new SideQuest("collector", "Collector", "Find 40 different items.", QuestTrigger.ItemsFound, 40, R(def: 1)),
            new SideQuest("legend_seeker", "Legend Seeker", "Find 5 different legendaries.", QuestTrigger.LegendariesFound, 5, R(crit: 0.03f)),
            new SideQuest("the_unsung", "The Unsung Five", "Reach B3 with every hero.", QuestTrigger.HeroesAtB3, 5, R(hp: 10)),
            new SideQuest("choir_breaker", "Choir Breaker", "Silence 5 different Choristers.", QuestTrigger.ChoristersSilenced, 5, R(atk: 3)),
        };

        public static readonly IReadOnlyList<Chapter> Main = BuildMain();

        static IReadOnlyList<Chapter> BuildMain()
        {
            var list = new List<Chapter>
            {
                new Chapter("prologue", "Prologue: The Buried Sun", "Hear the Sun's call.",
                            "Nine hundred years ago the Choir of Dawn sang the Sun into the earth. Now she sings back, and you answered."),
            };
            string[] verses = { "Bone", "Rot", "Tide", "Flame", "Silence", "Growth", "Measure", "Memory", "Reflection", "Hunger" };
            for (int floor = 1; floor <= 10; floor++)
            {
                var boss = Bestiary.BossOf(floor);
                list.Add(new Chapter("chapter_" + floor, $"Chapter {floor}: The Verse of {verses[floor - 1]}",
                                     $"Silence {boss.Name} in the {Dungeon.EncounterData.BiomeName(floor)}.",
                                     boss.Lore + " The verse is broken; the Sun is one step closer."));
            }
            list.Add(new Chapter("epilogue", "Epilogue: Undersun", "Break the last verse and raise the Sun.",
                                 "The hymn is unsung. Light climbs the shaft you carved through the dark, and the dusk above finally ends."));
            return list;
        }

        public static SideQuest Get(string id) => Side.FirstOrDefault(q => q.Id == id);

        /// <summary>Main-quest completion is derived from the save: codex unlocks of each Chorister and victories.</summary>
        public static bool IsDone(Chapter c, SaveData save)
        {
            if (c.Id == "prologue") return save.introSeen;
            if (c.Id == "epilogue") return save.victories > 0;
            int floor = int.Parse(c.Id.Substring("chapter_".Length));
            return save.IsCodexUnlocked(Bestiary.BossOf(floor).Id);
        }

        /// <summary>Adds progress, never past the goal. Returns true the moment the quest becomes complete.</summary>
        public static bool Advance(SaveData save, SideQuest quest, int amount)
        {
            var p = save.Quest(quest.Id);
            if (p.completed || amount <= 0) return false;
            p.progress = System.Math.Min(quest.Goal, p.progress + amount);
            if (p.progress < quest.Goal) return false;
            p.completed = true;
            return true;
        }

        /// <summary>For goals measured as a total (items found, choristers, heroes), sets progress to that total.</summary>
        public static bool SetProgress(SaveData save, SideQuest quest, int total) => Advance(save, quest, total - save.Quest(quest.Id).progress);

        /// <summary>Claims a completed quest's reward into the permanent bonuses. False if not claimable.</summary>
        public static bool Claim(SaveData save, SideQuest quest)
        {
            var p = save.Quest(quest.Id);
            if (!p.completed || p.claimed) return false;
            p.claimed = true;
            var r = quest.Reward;
            save.meta.health += r.Health;
            save.meta.attack += r.Attack;
            save.meta.defense += r.Defense;
            save.meta.startGold += r.StartGold;
            save.meta.crit += r.Crit;
            save.meta.goldFind += r.GoldFind;
            save.meta.skillHaste += r.SkillHaste;
            return true;
        }
    }
}
