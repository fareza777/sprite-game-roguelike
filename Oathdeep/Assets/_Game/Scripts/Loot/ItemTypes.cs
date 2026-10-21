using UnityEngine;

namespace Oathdeep.Loot
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    public enum EquipSlot { Weapon, Armor, Charm, Relic }

    /// <summary>Effects that are not plain stats. Summed across equipped items like a StatBlock.</summary>
    public readonly struct ItemSpecials
    {
        public readonly float Lifesteal;    // fraction of damage dealt returned as health
        public readonly float SkillHaste;   // fraction removed from the skill cooldown
        public readonly float GoldFind;     // extra gold, as a fraction
        public readonly int Thorns;         // flat damage returned to an attacker in reach

        public ItemSpecials(float lifesteal, float skillHaste, float goldFind, int thorns)
        {
            Lifesteal = lifesteal; SkillHaste = skillHaste; GoldFind = goldFind; Thorns = thorns;
        }

        public static readonly ItemSpecials None = default;

        public static ItemSpecials operator +(ItemSpecials a, ItemSpecials b) =>
            new ItemSpecials(a.Lifesteal + b.Lifesteal, a.SkillHaste + b.SkillHaste, a.GoldFind + b.GoldFind, a.Thorns + b.Thorns);

        /// <summary>Caps so stacked gear never breaks the game (no free skills, no immortality).</summary>
        public ItemSpecials Clamped() => new ItemSpecials(
            Mathf.Clamp(Lifesteal, 0f, 0.25f), Mathf.Clamp(SkillHaste, 0f, 0.6f), Mathf.Clamp(GoldFind, 0f, 3f), Mathf.Max(0, Thorns));

        public bool IsEmpty => Lifesteal <= 0f && SkillHaste <= 0f && GoldFind <= 0f && Thorns <= 0;
    }

    public static class RarityInfo
    {
        public static Color ColorOf(Rarity r) => r switch
        {
            Rarity.Common => new Color32(0xC7, 0xCF, 0xCC, 0xFF),
            Rarity.Uncommon => new Color32(0x75, 0xA7, 0x43, 0xFF),
            Rarity.Rare => new Color32(0x4F, 0x8F, 0xBA, 0xFF),
            Rarity.Epic => new Color32(0xA2, 0x3E, 0x8C, 0xFF),
            Rarity.Legendary => new Color32(0xDE, 0x9E, 0x41, 0xFF),
            _ => Color.white,
        };

        public static string Hex(Rarity r) => "#" + ColorUtility.ToHtmlStringRGB(ColorOf(r));

        /// <summary>Random affixes rolled on top of the base. Legendaries carry their own fixed power instead.</summary>
        public static int AffixCount(Rarity r) => r switch
        {
            Rarity.Uncommon => 1,
            Rarity.Rare => 2,
            Rarity.Epic => 3,
            Rarity.Legendary => 1,
            _ => 0,
        };

        public static float PowerMultiplier(Rarity r) => r switch
        {
            Rarity.Uncommon => 1.12f,
            Rarity.Rare => 1.25f,
            Rarity.Epic => 1.4f,
            Rarity.Legendary => 1.6f,
            _ => 1f,
        };

        public static int SellBase(Rarity r) => r switch
        {
            Rarity.Uncommon => 12,
            Rarity.Rare => 24,
            Rarity.Epic => 45,
            Rarity.Legendary => 90,
            _ => 5,
        };
    }
}
