using System.Collections.Generic;
using System.Text;
using Oathdeep.Data;

namespace Oathdeep.Loot
{
    /// <summary>One rolled item: a base identity + rarity + level, with its final stats and affixes baked in. Immutable.</summary>
    public sealed class ItemInstance
    {
        public string BaseId { get; }
        public Rarity Rarity { get; }
        public int Level { get; }
        public StatBlock Stats { get; }
        public ItemSpecials Specials { get; }
        public IReadOnlyList<string> AffixIds { get; }
        public string Name { get; }

        public ItemBase Base => ItemCatalog.Get(BaseId);
        public EquipSlot Slot => Base.Slot;
        public string Icon => Base.Icon;

        public ItemInstance(string baseId, Rarity rarity, int level, StatBlock stats, ItemSpecials specials, IReadOnlyList<string> affixIds, string name)
        {
            BaseId = baseId; Rarity = rarity; Level = level; Stats = stats; Specials = specials; AffixIds = affixIds; Name = name;
        }

        /// <summary>One number to compare gear by. Weights reflect how much each stat changes a fight.</summary>
        public int Power => PowerOf(Stats, Specials);

        public static int PowerOf(StatBlock s, ItemSpecials x) => (int)(
            s.Attack * 3f + s.MaxHealth * 0.5f + s.Defense * 3f + s.CritChance * 150f + s.CritMultiplier * 20f + s.MoveSpeed * 15f +
            x.Lifesteal * 300f + x.SkillHaste * 100f + x.GoldFind * 20f + x.Thorns * 2f);

        public int SellValue => RarityInfo.SellBase(Rarity) + Level * 3;

        /// <summary>Stat lines for tooltips, e.g. "+12 Attack", "+5% Crit chance".</summary>
        public static List<string> Describe(StatBlock s, ItemSpecials x)
        {
            var lines = new List<string>();
            void Flat(int v, string label) { if (v != 0) lines.Add($"{(v > 0 ? "+" : "")}{v} {label}"); }
            void Pct(float v, string label) { if (System.Math.Abs(v) >= 0.005f) lines.Add($"{(v > 0 ? "+" : "")}{v * 100f:0}% {label}"); }
            Flat(s.Attack, "Attack");
            Flat(s.MaxHealth, "Health");
            Flat(s.Defense, "Defense");
            Pct(s.CritChance, "Crit chance");
            Pct(s.CritMultiplier, "Crit damage");
            if (System.Math.Abs(s.MoveSpeed) >= 0.01f) lines.Add($"{(s.MoveSpeed > 0 ? "+" : "")}{s.MoveSpeed:0.0#} Speed");
            Pct(x.Lifesteal, "Lifesteal");
            Pct(x.SkillHaste, "Skill haste");
            Pct(x.GoldFind, "Gold find");
            Flat(x.Thorns, "Thorns");
            return lines;
        }

        public string Tooltip()
        {
            var sb = new StringBuilder();
            foreach (var line in Describe(Stats, Specials)) sb.AppendLine(line);
            return sb.ToString().TrimEnd();
        }
    }
}
