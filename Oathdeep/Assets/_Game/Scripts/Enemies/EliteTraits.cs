using Oathdeep.Data;
using UnityEngine;

namespace Oathdeep.Enemies
{
    /// <summary>What makes an elite more than a bigger enemy. Each shows as its own aura colour.</summary>
    public enum EliteTrait { Swift, Armored, Giant, Regenerating }

    public readonly struct EliteBuff
    {
        public readonly StatBlock Stats;
        public readonly float DamageTaken;   // Armored shrugs off part of every hit
        public readonly float Size;
        public readonly Color Aura;

        public EliteBuff(StatBlock stats, float damageTaken, float size, Color aura) { Stats = stats; DamageTaken = damageTaken; Size = size; Aura = aura; }
    }

    public static class EliteTraits
    {
        public static EliteTrait Pick(double roll)
        {
            int n = System.Enum.GetValues(typeof(EliteTrait)).Length;
            return (EliteTrait)Mathf.Clamp((int)(roll * n), 0, n - 1);
        }

        public static EliteBuff Apply(StatBlock s, EliteTrait trait) => trait switch
        {
            EliteTrait.Swift => new EliteBuff(With(s, s.MaxHealth, s.MoveSpeed * 1.4f), 1f, 1f, new Color(0.45f, 0.9f, 1f)),
            EliteTrait.Armored => new EliteBuff(s, 0.6f, 1f, new Color(0.7f, 0.75f, 0.85f)),
            EliteTrait.Giant => new EliteBuff(With(s, Mathf.RoundToInt(s.MaxHealth * 1.5f), s.MoveSpeed * 0.85f), 1f, 1.3f, new Color(1f, 0.4f, 0.3f)),
            _ => new EliteBuff(s, 1f, 1f, new Color(0.5f, 1f, 0.45f)),   // Regenerating: heals while left alone
        };

        static StatBlock With(StatBlock s, int health, float speed) =>
            new StatBlock(health, s.Attack, s.Defense, s.CritChance, s.CritMultiplier, speed);
    }
}
