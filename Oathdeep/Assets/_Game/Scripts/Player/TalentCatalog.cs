using System.Collections.Generic;
using System.Linq;

namespace Oathdeep.Player
{
    /// <summary>Run-long talents that change how the hero fights (offered at sunstones). They combine:
    /// a fiery, thorny roll; sparks that chain into bursting kills.</summary>
    public enum Talent { EmberTrail, ThornRoll, ChainSpark, DeathBurst, Retaliate, SecondWind }

    public readonly struct TalentInfo
    {
        public readonly Talent Id;
        public readonly string Name;
        public readonly string Text;

        public TalentInfo(Talent id, string name, string text) { Id = id; Name = name; Text = text; }
    }

    public static class TalentCatalog
    {
        public static readonly IReadOnlyList<TalentInfo> All = new[]
        {
            new TalentInfo(Talent.EmberTrail, "Ember Trail", "Rolling leaves a trail of fire."),
            new TalentInfo(Talent.ThornRoll, "Thorn Roll", "Rolling through foes cuts them."),
            new TalentInfo(Talent.ChainSpark, "Chain Spark", "Every third hit sparks to two more foes."),
            new TalentInfo(Talent.DeathBurst, "Death Burst", "Foes you slay burst, hurting their friends."),
            new TalentInfo(Talent.Retaliate, "Retaliate", "Taking a hit sends out a shockwave."),
            new TalentInfo(Talent.SecondWind, "Second Wind", "Once a floor, near death, heal 30%."),
        };

        public static TalentInfo Info(Talent t) => All.First(i => i.Id == t);

        /// <summary>A talent the hero does not have yet, or null once they have them all.</summary>
        public static Talent? Draw(System.Random rng, ICollection<Talent> owned)
        {
            var open = All.Where(i => !owned.Contains(i.Id)).ToList();
            if (open.Count == 0) return null;
            return open[rng.Next(open.Count)].Id;
        }

        public static bool ChainSparkFires(int hitCount) => hitCount > 0 && hitCount % 3 == 0;

        public static bool SecondWindDue(int current, int max, bool usedThisFloor) => !usedThisFloor && current > 0 && current * 4 < max;
    }
}
