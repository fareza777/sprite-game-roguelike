using System;
using System.Collections.Generic;
using System.Linq;
using Oathdeep.Data;

namespace Oathdeep.Loot
{
    /// <summary>Procedural loot. Deterministic for a given System.Random so drops can be tested and replayed.</summary>
    public static class ItemGenerator
    {
        /// <summary>Flat stats (attack, health, defense, thorns) grow this much per floor below the first.</summary>
        public const float FloorGrowth = 0.2f;
        public const float QualityMin = 0.9f, QualityMax = 1.1f;

        // Common, Uncommon, Rare, Epic, Legendary at floor 1.
        static readonly float[] BaseWeights = { 62f, 25f, 9.5f, 3f, 0.5f };

        /// <summary>Rarity weights shift toward the top with depth; <paramref name="luck"/> (elites, chests, bosses) shifts further.</summary>
        public static Rarity RollRarity(Random rng, int floor, float luck = 0f)
        {
            float depth = Math.Max(0, floor - 1) * 0.12f + luck;
            var w = new float[5];
            for (int i = 0; i < 5; i++) w[i] = BaseWeights[i] * (float)Math.Pow(1f + depth, i);
            double roll = rng.NextDouble() * w.Sum();
            for (int i = 0; i < 5; i++)
            {
                roll -= w[i];
                if (roll <= 0) return (Rarity)i;
            }
            return Rarity.Legendary;
        }

        public static ItemInstance Roll(Random rng, int floor, float luck = 0f)
        {
            var rarity = RollRarity(rng, floor, luck);
            var pool = rarity == Rarity.Legendary
                ? ItemCatalog.Legendaries.Where(b => b.MinFloor <= floor).ToList()
                : ItemCatalog.Bases.Where(b => b.MinFloor <= floor + 1).ToList();
            if (pool.Count == 0) pool = ItemCatalog.Bases.Where(b => b.MinFloor <= 1).ToList();
            // Favour items themed for this depth over leftovers from the first floors.
            var weights = pool.Select(b => 1f + Math.Max(0f, 3f - Math.Abs(floor - b.MinFloor))).ToArray();
            return Create(pool[Pick(rng, weights)], rarity, floor, rng);
        }

        /// <summary>Rolls a specific base (chests of a theme, quest rewards, tests).</summary>
        public static ItemInstance Create(ItemBase item, Rarity rarity, int floor, Random rng)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Legendary) rarity = Rarity.Legendary;
            floor = Math.Max(1, floor);
            float quality = QualityMin + (float)rng.NextDouble() * (QualityMax - QualityMin);
            float flat = (1f + FloorGrowth * (floor - 1)) * RarityInfo.PowerMultiplier(rarity) * quality;
            float soft = RarityInfo.PowerMultiplier(rarity) * quality;

            var stats = Scale(item.Stats, flat, soft);
            var specials = ScaleSpecials(item.Specials, flat, soft);

            var affixes = new List<string>();
            int count = RarityInfo.AffixCount(rarity);
            var available = ItemCatalog.Affixes.ToList();
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                var affix = available[rng.Next(available.Count)];
                available.Remove(affix);
                affixes.Add(affix.Id);
                float t = 0.5f + 0.5f * (float)rng.NextDouble();   // half to full strength
                float affixFlat = t * (1f + FloorGrowth * 0.75f * (floor - 1));
                stats += Scale(affix.Stats, affixFlat, t);
                specials += ScaleSpecials(affix.Specials, affixFlat, t);
            }

            return new ItemInstance(item.Id, rarity, floor, stats, specials, affixes, NameFor(item, affixes));
        }

        static string NameFor(ItemBase item, List<string> affixIds)
        {
            if (item.Legendary) return item.Name;
            var defs = affixIds.Select(ItemCatalog.Affix).ToList();
            var prefix = defs.FirstOrDefault(a => a.IsPrefix);
            var suffix = defs.FirstOrDefault(a => !a.IsPrefix);
            string name = item.Name;
            if (prefix != null) name = prefix.Name + " " + name;
            if (suffix != null) name = name + " " + suffix.Name;
            return name;
        }

        /// <summary>Flat stats scale with depth; percentages only with rarity and quality so they never run away.</summary>
        static StatBlock Scale(StatBlock s, float flat, float soft) => new StatBlock(
            Round(s.MaxHealth * flat), Round(s.Attack * flat), Round(s.Defense * flat),
            s.CritChance * soft, s.CritMultiplier * soft, s.MoveSpeed * soft);

        static ItemSpecials ScaleSpecials(ItemSpecials x, float flat, float soft) => new ItemSpecials(
            x.Lifesteal * soft, x.SkillHaste * soft, x.GoldFind * soft, Round(x.Thorns * flat));

        /// <summary>Rounds away from zero so a non-zero base never rolls to nothing.</summary>
        static int Round(float v) => v == 0f ? 0 : v > 0f ? Math.Max(1, (int)Math.Round(v)) : Math.Min(-1, (int)Math.Round(v));

        static int Pick(Random rng, float[] weights)
        {
            double roll = rng.NextDouble() * weights.Sum();
            for (int i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0) return i;
            }
            return weights.Length - 1;
        }
    }
}
